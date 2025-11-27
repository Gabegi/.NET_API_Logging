namespace LoggingProduction.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string CorrelationIdLogKey = "CorrelationId";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        context.Items[CorrelationIdLogKey] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        using (_logger.BeginScope(new Dictionary<string, object> { { CorrelationIdLogKey, correlationId } }))
        {
            _logger.LogInformation("Request started with correlation ID: {CorrelationId}", correlationId);
            await _next(context);
            _logger.LogInformation("Request completed with correlation ID: {CorrelationId}", correlationId);
        }
    }
}
