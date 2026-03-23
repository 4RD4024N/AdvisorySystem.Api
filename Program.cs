using System.Text;
using System.Threading.RateLimiting;
using AdvisorySystem.Api.Data;
using AdvisorySystem.Api.Middleware;
using AdvisorySystem.Api.Models;
using AdvisorySystem.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
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

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Global limiter - fallback for all endpoints
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        factory: partition => new FixedWindowRateLimiterOptions
            {
             AutoReplenishment = true,
             PermitLimit = 100,
         Window = TimeSpan.FromMinutes(1)
            }));

    // Auth-strict policy (login, register)
    options.AddFixedWindowLimiter("auth-strict", opt =>
    {
 opt.PermitLimit = 5;
    opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    
    // Auth-relaxed policy (refresh, validate)
    options.AddFixedWindowLimiter("auth-relaxed", opt =>
    {
     opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

 // Upload policy
    options.AddFixedWindowLimiter("upload", opt =>
    {
      opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
   opt.QueueLimit = 2;
    });

    // Download policy
    options.AddFixedWindowLimiter("download", opt =>
    {
  opt.PermitLimit = 50;
      opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Search policy
    options.AddSlidingWindowLimiter("search", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
    opt.SegmentsPerWindow = 6; // 10-second segments
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 3;
    });

    // Standard CRUD policy
    options.AddFixedWindowLimiter("standard", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Admin policy (more relaxed)
    options.AddFixedWindowLimiter("admin", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    // Custom response for rate limit exceeded
    options.OnRejected = async (context, token) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        
        logger.LogWarning(
     "Rate limit exceeded for {Endpoint} by {User} from {IP}",
    context.HttpContext.Request.Path,
    context.HttpContext.User.Identity?.Name ?? "anonymous",
  context.HttpContext.Connection.RemoteIpAddress);

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
   
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
  await context.HttpContext.Response.WriteAsJsonAsync(new
   {
          error = "Too many requests",
  message = "Rate limit exceeded. Please try again later.",
  retryAfter = retryAfter.TotalSeconds
     }, cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
      {
   error = "Too many requests",
 message = "Rate limit exceeded. Please try again later."
  }, cancellationToken: token);
        }
    };
});

// CORS (frontend: Vite 5173, 5174, 5175, 44375)
builder.Services.AddCors(o =>
{
  o.AddPolicy("frontend", p => p
.WithOrigins(
      "http://localhost:5173", 
    "http://localhost:5174",
 "http://localhost:5175",    
        "http://localhost:3000",
        "https://localhost:44375"   
        ) 
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
builder.Services.AddScoped<ICourseScheduler, CourseScheduler>();

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
    await CourseSeeder.SeedCoursesAsync(app.Services);
    await CourseScheduleSeeder.SeedSchedulesAsync(app.Services); 
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error while seeding data");
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("frontend");

// File size validation middleware
app.UseFileSizeValidation();

// Rate limiting middleware - MUST be after UseRouting, BEFORE UseAuthentication
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
