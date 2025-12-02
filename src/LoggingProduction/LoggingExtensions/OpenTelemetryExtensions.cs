using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LoggingProduction.LoggingExtensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry with Elastic APM
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing with Elastic APM export
    /// </summary>
    public static IServiceCollection AddOpenTelemetryWithElasticAPM(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "LoggingProduction"))

            // Configure Traces
            .WithTracing(tracing => tracing
                // Instrument ASP.NET Core (HTTP requests)
                .AddAspNetCoreInstrumentation(options =>
                {
                    // Filter out health checks from tracing
                    options.Filter = httpContext =>
                        !httpContext.Request.Path.StartsWithSegments("/health");
                })
                // Instrument outgoing HTTP calls
                .AddHttpClientInstrumentation()
                // Export traces to Elastic APM Server via OTLP
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:8200");
                }));

        return services;
    }
}
