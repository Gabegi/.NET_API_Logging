using Microsoft.Extensions.Logging;

namespace LoggingProduction.Data.Repositories;

/// <summary>
/// Source-generated logger for OrderRepository
/// Uses [LoggerMessage] for compile-time optimization: zero allocations, no boxing
/// </summary>
public static partial class OrderRepositoryLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching all orders from repository")]
    public static partial void LogFetchingAllOrders(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {Count} orders from repository")]
    public static partial void LogRetrievedOrders(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching order {OrderId} from repository")]
    public static partial void LogFetchingOrder(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} found in repository")]
    public static partial void LogOrderFound(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} not found in repository")]
    public static partial void LogOrderNotFound(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Saving order {OrderId} to repository for customer {CustomerId} with total {Total}")]
    public static partial void LogSavingOrder(ILogger logger, string orderId, string customerId, decimal total);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} saved successfully")]
    public static partial void LogOrderSavedSuccessfully(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database connection timeout while saving order {OrderId}")]
    public static partial void LogDatabaseTimeout(ILogger logger, Exception ex, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Searching orders with customer filter: {CustomerId}")]
    public static partial void LogSearchingOrders(ILogger logger, string? customerId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Found {OrderCount} orders matching criteria")]
    public static partial void LogOrdersMatched(ILogger logger, int orderCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating order {OrderId} in repository")]
    public static partial void LogUpdatingOrder(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} updated in repository")]
    public static partial void LogOrderUpdated(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting order {OrderId} from repository")]
    public static partial void LogDeletingOrder(ILogger logger, string orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} deleted from repository")]
    public static partial void LogOrderDeleted(ILogger logger, string orderId);
}
