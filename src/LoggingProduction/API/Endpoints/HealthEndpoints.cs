namespace LoggingProduction.API.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", GetHealth)
            .WithName("Health")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .ExcludeFromDescription();

        app.MapGet("/health/ready", GetReadiness)
            .WithName("Readiness")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .ExcludeFromDescription();
    }

    private static IResult GetHealth(ILogger<Program> logger)
    {
        logger.LogInformation("Health check requested");
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    private static IResult GetReadiness(ILogger<Program> logger)
    {
        logger.LogInformation("Readiness check requested");
        return Results.Ok(new { ready = true, timestamp = DateTime.UtcNow });
    }
}
