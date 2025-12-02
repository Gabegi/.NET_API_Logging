using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LoggingProduction.LoggingExtensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing with environment-based export
    /// Development: Console exporter (for debugging)
    /// Production: Elastic APM Server via OTLP
    /// </summary>
    public static IServiceCollection AddOpenTelemetryWithEnvironmentExport(this IServiceCollection services, IHostEnvironment env)
    {
        var tracingBuilder = services.AddOpenTelemetry()
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
                .AddHttpClientInstrumentation());

        // Environment-specific export configuration
        if (env.IsDevelopment())
        {
            // Development: Export traces to console for easy debugging
            tracingBuilder.WithTracing(tracing => tracing
                .AddConsoleExporter());
        }
        else
        {
            // Production: Export traces to Elastic APM Server via OTLP gRPC
            tracingBuilder.WithTracing(tracing => tracing
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4317");
                }));
        }

        return services;
    }
}
