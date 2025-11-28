using LoggingProduction.API.Endpoints;
using LoggingProduction.API.Middleware;
using LoggingProduction.Data.Repositories;
using LoggingProduction.Services;
using LoggingProduction.LoggingExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the logging provider
// This replaces the default .NET logging with Serilog
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

        // Enrich logs with additional context
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "LoggingProduction")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId());

builder.Services.AddOpenApi();

// Register repositories
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLoggingConfiguration();

// Map endpoints
app.MapProductEndpoints();
app.MapOrderEndpoints();
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
