using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace NG.MicroERP.API.Middleware
{
    /// <summary>
    /// Extension method to add rate limiting and security policies
    /// </summary>
    public static class SecurityExtensions
    {
        public static IServiceCollection AddSecurityPolicies(this IServiceCollection services)
        {
            // Add rate limiting
            services.AddRateLimiter(limiterOptions =>
            {
                limiterOptions.AddFixedWindowLimiter(policyName: "fixed", options =>
                {
                    options.PermitLimit = 100;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                });

                limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            return services;
        }

        public static WebApplication UseSecurityMiddleware(this WebApplication app)
        {
            // Add security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
                
                await next();
            });

            app.UseRateLimiter();

            return app;
        }
    }
}
