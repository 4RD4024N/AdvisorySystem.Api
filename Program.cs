using System.Text;
using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Middleware;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Application Insights (if configured)
var appInsightsConnectionString = builder.Configuration["Azure:ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// Db
builder.Services.AddDbContext<AppDbContext>(o =>
 o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// CORS (frontend: Vite 5173)
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:5173", "http://localhost:3000") 
  .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidateAudience = true,
   ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
   {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ctx.Exception, "JWT authentication failed");
   return Task.CompletedTask;
        },
        OnMessageReceived = ctx =>
        {
          return Task.CompletedTask;
        },
 OnChallenge = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
       logger.LogWarning("JWT OnChallenge: {0}", ctx.ErrorDescription);
            return Task.CompletedTask;
        }
  };
});

// Storage Service - Choose between Local or Azure based on configuration
var azureStorageConnectionString = builder.Configuration["Azure:StorageConnectionString"];
if (!string.IsNullOrEmpty(azureStorageConnectionString))
{
  builder.Services.AddScoped<IFileStorage, AzureBlobStorage>();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    Console.WriteLine("Using Azure Blob Storage");
}
else
{
    builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
    Console.WriteLine("Using Local File Storage");
}

builder.Services.AddScoped<INotificationService, NotificationService>();

// Background service for deadline notifications
builder.Services.AddHostedService<DeadlineNotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
   Description = "Enter the JWT token only (do NOT include the 'Bearer ' prefix).",
  Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
   Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "bearer",
     Name = "Bearer",
                In = ParameterLocation.Header
          },
       Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed identity data (roles/admin)
try
{
    await IdentitySeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error while seeding identity data");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("frontend");

// File size validation middleware
app.UseFileSizeValidation();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
