using Microsoft.Extensions.Logging;

namespace LoggingProduction.Telemetry;

/// <summary>
/// Source-generated logger for ProductRepository
/// Uses [LoggerMessage] for compile-time optimization: zero allocations, no boxing
/// </summary>
public static partial class ProductRepositoryLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching all products from repository")]
    public static partial void LogFetchingAllProducts(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {Count} products from repository")]
    public static partial void LogRetrievedProducts(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching product {ProductId} from repository")]
    public static partial void LogFetchingProduct(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} found in repository")]
    public static partial void LogProductFound(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} not found in repository")]
    public static partial void LogProductNotFound(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Saving product {ProductId} to repository with name {ProductName} and price {Price}")]
    public static partial void LogSavingProduct(ILogger logger, string productId, string productName, decimal price);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} saved successfully")]
    public static partial void LogProductSavedSuccessfully(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Database connection timeout while saving product {ProductId}")]
    public static partial void LogDatabaseTimeout(ILogger logger, Exception ex, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating product {ProductId} in repository")]
    public static partial void LogUpdatingProduct(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} updated in repository")]
    public static partial void LogProductUpdated(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting product {ProductId} from repository")]
    public static partial void LogDeletingProduct(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} deleted from repository")]
    public static partial void LogProductDeleted(ILogger logger, string productId);
}
