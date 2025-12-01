using LoggingProduction.API.Endpoints;
using LoggingProduction.API.Middleware;
using LoggingProduction.Data.Repositories;
using LoggingProduction.Services;
using LoggingProduction.LoggingExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the logging provider
builder.ConfigureSerilog();

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

// Wrap app.RunAsync() in try-catch to log startup/shutdown events
// This ensures we capture critical application lifecycle logs
try
{
    Log.Information("Application starting {@Application}", new {
        Application = "LoggingProduction",
        Environment = app.Environment.EnvironmentName,
        MachineName = Environment.MachineName
    });

    // Start the web application
    await app.RunAsync();
}
catch (Exception ex)
{
    // Log fatal errors that crash the application
    // Critical for diagnosing startup failures
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    // Flush any buffered logs before app exits
    // Without this, last few logs might be lost
    await Log.CloseAndFlushAsync();
}
