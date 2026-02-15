using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MicroERP.API.Helper;
using MicroERP.API.HealthChecks;
using MicroERP.API.Middleware;
using MicroERP.API.Services;
using MicroERP.API.Services.Services;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MicroERP.API")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// JWT Authentication - Get credentials from environment variables for security
string? key = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
string? issuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER");
string? audience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    Log.Warning("JWT credentials not configured. Please set environment variables: JWT_KEY, JWT_ISSUER, JWT_AUDIENCE");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? string.Empty)),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            ClockSkew = TimeSpan.FromSeconds(5)
        };
        
        // Add event handlers for debugging authentication failures
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var username = context.Principal?.Identity?.Name ?? 
                              context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ??
                              context.Principal?.FindFirst("sub")?.Value ?? 
                              "Unknown";
                Log.Debug("JWT Token validated for user: {Username}", username);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var requestPath = context.Request.Path;
                var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
                Log.Warning("JWT Challenge - Path: {Path}, HasAuth: {HasAuth}, Error: {Error}", 
                    requestPath, hasAuthHeader, context.Error);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<JWT>();

// Configure CORS with restricted origins instead of AllowAll
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') ?? 
                         Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? 
                         new[] { "http://localhost:5000", "https://localhost:5001" };

    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add security policies
builder.Services.AddSecurityPolicies();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("api_health");

builder.Services.AddSingleton<DatabaseMigrator>();

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});

var app = builder.Build();

// Middleware
app.UseResponseCompression();
app.UseSecurityMiddleware();

app.UseRouting();

// Use appropriate CORS policy based on environment
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "AllowSpecificOrigins";
app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseHttpsRedirection();

// Add health check endpoint
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroERP API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

// Database Migration
using (var scope = app.Services.CreateScope())
{
    try
    {
        var migrator = scope.ServiceProvider.GetRequiredService<DatabaseMigrator>();
        migrator.Migrate();
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration failed");
    }
}

app.Run();

