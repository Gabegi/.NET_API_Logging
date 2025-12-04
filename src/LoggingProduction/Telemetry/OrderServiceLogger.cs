using Microsoft.Extensions.Logging;

namespace LoggingProduction.Telemetry;

/// <summary>
/// Source-generated logger for OrderService
/// Uses [LoggerMessage] for compile-time optimization: zero allocations, no boxing
/// </summary>
public static partial class OrderServiceLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieving all orders")]
    public static partial void LogRetrievingAllOrders(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieving order {OrderId}")]
    public static partial void LogRetrievingOrder(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating order for customer {CustomerId} with total {Total}")]
    public static partial void LogCreatingOrder(ILogger logger, string customerId, decimal total);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} created successfully with status {Status}")]
    public static partial void LogOrderCreatedSuccessfully(ILogger logger, string orderId, string status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create order {OrderId} due to timeout")]
    public static partial void LogOrderCreationTimeout(ILogger logger, Exception ex, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Searching orders with customerId filter: {CustomerId}")]
    public static partial void LogSearchingOrders(ILogger logger, string? customerId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Found {OrderCount} orders")]
    public static partial void LogOrdersFound(ILogger logger, int orderCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating order {OrderId}")]
    public static partial void LogUpdatingOrder(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} updated successfully")]
    public static partial void LogOrderUpdatedSuccessfully(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Order {OrderId} not found for update")]
    public static partial void LogOrderNotFoundForUpdate(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting order {OrderId}")]
    public static partial void LogDeletingOrder(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} deleted successfully")]
    public static partial void LogOrderDeletedSuccessfully(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Order {OrderId} not found for deletion")]
    public static partial void LogOrderNotFoundForDeletion(ILogger logger, string orderId);
}
