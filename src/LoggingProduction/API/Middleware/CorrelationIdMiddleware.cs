using Serilog.Context;

namespace LoggingProduction.API.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string CorrelationIdLogKey = "CorrelationId";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get correlation ID from request header or generate new one
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        // Store in HttpContext.Items for later access
        context.Items[CorrelationIdLogKey] = correlationId;

        // Add to response headers so client can reference it
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        // Push to Serilog LogContext - this adds CorrelationId to ALL logs in this request
        using (LogContext.PushProperty(CorrelationIdLogKey, correlationId))
        {
            await _next(context);
        }
    }
}
