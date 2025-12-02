using System.Diagnostics;

namespace LoggingProduction.Telemetry;

/// <summary>
/// Centralized ActivitySource for distributed tracing
/// </summary>
public static class ActivitySourceProvider
{
    public static readonly ActivitySource Source = new("LoggingProduction", "1.0.0");
}
