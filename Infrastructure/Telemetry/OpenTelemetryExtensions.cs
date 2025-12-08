using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LoggingProduction.Infrastructure.Telemetry;

/// <summary>
/// Extension methods for configuring OpenTelemetry
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics with environment-based export
    /// Development: Console exporter (for debugging)
    /// Production: Elastic APM Server via OTLP
    /// </summary>
    public static IServiceCollection AddOpenTelemetryWithEnvironmentExport(this IServiceCollection services, IHostEnvironment env)
    {
        var builder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "LoggingProduction"))

            // Configure Metrics
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()      // HTTP request metrics
                .AddRuntimeInstrumentation()         // Memory, GC, threads
                .AddHttpClientInstrumentation())     // Outgoing HTTP calls

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
            // Development: Export traces and metrics to console for easy debugging
            builder.WithTracing(tracing => tracing.AddConsoleExporter());
            builder.WithMetrics(metrics => metrics.AddConsoleExporter());
        }
        else
        {
            // Production: Export traces and metrics to Elastic APM Server via OTLP gRPC
            builder.WithTracing(tracing => tracing
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4317");
                }));

            builder.WithMetrics(metrics => metrics
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4317");
                }));
        }

        return services;
    }
}
