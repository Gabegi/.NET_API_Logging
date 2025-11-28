using Serilog;
using Serilog.Events;

namespace LoggingProduction.LoggingExtensions;

/// <summary>
/// Extension methods for configuring Serilog request/response logging
/// </summary>
public static class RequestLoggingExtensions
{
    /// <summary>
    /// Configures Serilog middleware with smart log level filtering and enrichment
    /// </summary>
    public static WebApplication UseSerilogRequestLoggingConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            // Smart log level filtering
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                // Health checks → Hidden (Verbose level - won't appear unless explicitly configured)
                if (httpContext.Request.Path.StartsWithSegments("/health"))
                    return LogEventLevel.Verbose;

                // Server errors (500+) → Error
                if (ex != null || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                // Client errors (400+) → Warning
                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                // Slow requests (>1 second) → Warning
                if (elapsed > 1000)
                    return LogEventLevel.Warning;

                // Normal requests → Information
                return LogEventLevel.Information;
            };

            // Enrich diagnostic context with request details
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown");
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);
                diagnosticContext.Set("ResponseStatusCode", httpContext.Response.StatusCode);

                // Include correlation ID if available
                if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }
            };

        });

        return app;
    }
}
