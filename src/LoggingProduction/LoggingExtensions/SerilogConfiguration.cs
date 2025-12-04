using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Enrichers.Sensitive;
using Serilog.Enrichers.Sensitive.Models;

namespace LoggingProduction.LoggingExtensions;

/// <summary>
/// Extension methods for configuring Serilog logging
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog with Console, File, and Elasticsearch sinks
    /// </summary>
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
            configuration
                // Read settings from appsettings.json (log levels, etc.)
                .ReadFrom.Configuration(context.Configuration)

                // Write logs to console with formatted output
                .WriteTo.Async(a => a.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))

                // Write logs to daily rolling files with formatted output
                // Creates a new file each day, prevents single huge log file
                .WriteTo.Async(a => a.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))

                // Write logs to Elasticsearch for centralized log aggregation
                // Requires Elasticsearch running on localhost:9200
                .WriteTo.Async(a => a.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                    {
                        // Index name pattern: logstash-YYYY.MM.DD (daily indices)
                        IndexFormat = "logstash-{0:yyyy.MM.dd}",

                        // Number of events to batch before sending to Elasticsearch
                        BatchPostingLimit = 100,

                        // Time to wait before sending partial batch
                        Period = TimeSpan.FromSeconds(5),

                        // Auto-register Elasticsearch index mapping (disabled to prevent blocking)
                        AutoRegisterTemplate = false,

                        // Template name (must match index pattern)
                        TemplateName = "logstash-template",

                        // Override default minimum log level for Elasticsearch
                        MinimumLogEventLevel = LogEventLevel.Information,

                        // Connection timeout to prevent blocking
                        ConnectionTimeout = TimeSpan.FromSeconds(5),

                        // Enable this to see detailed ES errors in logs
                        FailureCallback = (logEvent, exception) =>
                            Console.WriteLine($"Unable to submit event to Elasticsearch: {logEvent.MessageTemplate}")
                    }))

                // Enrich logs with additional context
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "LoggingProduction")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()

                // Mask sensitive data (PII) to prevent GDPR/PCI-DSS violations
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    // Mask email addresses: john@example.com -> j***@example.com
                    options.MaskProperties.Add(new MaskProperty { Name = "Email" });
                    options.MaskProperties.Add(new MaskProperty { Name = "email" });

                    // Mask credit card numbers: 4532-1234-5678-9010 -> ****-****-****-9010
                    options.MaskProperties.Add(new MaskProperty { Name = "CardNumber" });
                    options.MaskProperties.Add(new MaskProperty { Name = "CreditCard" });
                    options.MaskProperties.Add(new MaskProperty { Name = "Card" });

                    // Mask passwords and API keys
                    options.MaskProperties.Add(new MaskProperty { Name = "Password" });
                    options.MaskProperties.Add(new MaskProperty { Name = "ApiKey" });
                    options.MaskProperties.Add(new MaskProperty { Name = "Secret" });
                    options.MaskProperties.Add(new MaskProperty { Name = "Token" });

                    // Mask phone numbers
                    options.MaskProperties.Add(new MaskProperty { Name = "PhoneNumber" });
                    options.MaskProperties.Add(new MaskProperty { Name = "Phone" });

                    // Mask social security numbers
                    options.MaskProperties.Add(new MaskProperty { Name = "SSN" });
                    options.MaskProperties.Add(new MaskProperty { Name = "SocialSecurityNumber" });
                }));
    }
}
