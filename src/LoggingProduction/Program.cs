using LoggingProduction.Endpoints;
using LoggingProduction.Middleware;
using LoggingProduction.Repository;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the logging provider
// This replaces the default .NET logging with Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration
        // Read settings from appsettings.json (log levels, etc.)
        .ReadFrom.Configuration(context.Configuration)

        // Write logs to console (useful for development and Docker)
        .WriteTo.Console()

        // Write logs to daily rolling files (logs/log-20250127.txt)
        // Creates a new file each day, prevents single huge log file
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)

        // Enrich logs with additional context
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "LoggingProduction")
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId());

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();

// Map endpoints
app.MapProductEndpoints();
app.MapHealthEndpoints();

// Wrap app.Run() in try-catch to log startup/shutdown events
// This ensures we capture critical application lifecycle logs
try
{
    // Log successful startup (helps confirm app is running)
    Log.Information("Starting application");

    // Start the web application
    app.Run();
}
catch (Exception ex)
{
    // Log fatal errors that crash the application
    // Critical for diagnosing startup failures
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // Flush any buffered logs before app exits
    // Without this, last few logs might be lost
    Log.CloseAndFlush();
}
