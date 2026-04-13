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

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ── Application Insights ──────────────────────────────────────────────────────
var appInsightsConnectionString = builder.Configuration["Azure:ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ── Identity ──────────────────────────────────────────────────────────────────
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

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
      factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
       Window = TimeSpan.FromMinutes(1)
    }));

    options.AddFixedWindowLimiter("auth-strict", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

  options.AddFixedWindowLimiter("auth-relaxed", opt =>
    {
    opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
   opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    options.AddFixedWindowLimiter("upload", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    options.AddFixedWindowLimiter("download", opt =>
    {
        opt.PermitLimit = 50;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
  });

    options.AddSlidingWindowLimiter("search", opt =>
    {
   opt.PermitLimit = 30;
   opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 3;
    });

    options.AddFixedWindowLimiter("standard", opt =>
    {
        opt.PermitLimit = 60;
 opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.AddFixedWindowLimiter("admin", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.OnRejected = async (context, token) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit exceeded for {Endpoint} by {User} from {IP}",
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

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:5174",
      "http://localhost:5175", "http://localhost:3000",
      "https://localhost:44375", "https://nice-sand-008811f03.7.azurestaticapps.net"];

builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
      .WithOrigins(allowedOrigins)
     .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());

    o.AddPolicy("fallback", p => p
   .WithOrigins("https://nice-sand-008811f03.7.azurestaticapps.net")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection("Jwt");
var jwtKeyValue = jwt["Key"];
if (string.IsNullOrWhiteSpace(jwtKeyValue))
    throw new InvalidOperationException(
  "JWT Key is not configured. Set Jwt__Key environment variable in Azure App Service.");
var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyValue));

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer           = jwt["Issuer"],
        ValidAudience     = jwt["Audience"],
        IssuerSigningKey         = jwtKey,
        ValidateIssuer = true,
 ValidateAudience         = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime         = true,
   ClockSkew          = TimeSpan.Zero
    };

o.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ctx.Exception, "JWT authentication failed");
   return Task.CompletedTask;
        },
 OnMessageReceived = ctx => Task.CompletedTask,
   OnChallenge = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
       logger.LogWarning("JWT OnChallenge: {0}", ctx.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// ── File Storage ──────────────────────────────────────────────────────────────
var azureStorageConnectionString = builder.Configuration["Azure:StorageConnectionString"];
if (!string.IsNullOrEmpty(azureStorageConnectionString))
    builder.Services.AddScoped<IFileStorage, AzureBlobStorage>();
else
    builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICourseScheduler, CourseScheduler>();
builder.Services.AddHostedService<DeadlineNotificationService>();

// ── Controllers & Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description  = "Enter the JWT token only (do NOT include the 'Bearer ' prefix).",
        Name         = "Authorization",
        In           = ParameterLocation.Header,
     Type      = SecuritySchemeType.Http,
   Scheme       = "bearer",
      BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
   {
         new OpenApiSecurityScheme
            {
 Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
      Scheme    = "bearer",
      Name      = "Bearer",
     In     = ParameterLocation.Header
        },
     Array.Empty<string>()
      }
    });
});

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();

// ── Auto-migrate ──────────────────────────────────────────────────────────────
try
{
    using var migrateScope = app.Services.CreateScope();
    var db = migrateScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    startupLogger.LogInformation("Database migrations applied successfully");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "Failed to apply database migrations");
}

// ── Seeding ───────────────────────────────────────────────────────────────────
try
{
    await IdentitySeeder.SeedAsync(app.Services);
    await CourseSeeder.SeedCoursesAsync(app.Services);
    await CourseScheduleSeeder.SeedSchedulesAsync(app.Services);
    startupLogger.LogInformation("Seed data applied successfully");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "Error while seeding data");
}

// ── Storage log ───────────────────────────────────────────────────────────────
startupLogger.LogInformation("File storage provider: {Provider}",
    string.IsNullOrEmpty(azureStorageConnectionString) ? "LocalFileStorage" : "AzureBlobStorage");

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "frontend" : "fallback");
app.UseFileSizeValidation();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
