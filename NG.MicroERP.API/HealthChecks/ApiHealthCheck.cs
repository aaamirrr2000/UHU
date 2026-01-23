using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NG.MicroERP.API.HealthChecks
{
    /// <summary>
    /// Custom health check to monitor API status
    /// </summary>
    public class ApiHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Add custom health check logic here
                // For now, just check if the service is running
                return Task.FromResult(HealthCheckResult.Healthy("API is running normally"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"API health check failed: {ex.Message}"));
            }
        }
    }
}
