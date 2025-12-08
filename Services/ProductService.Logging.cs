using Microsoft.Extensions.Logging;

namespace LoggingProduction.Services;

/// <summary>
/// Source-generated logger for ProductService
/// Uses [LoggerMessage] for compile-time optimization: zero allocations, no boxing
/// </summary>
public static partial class ProductServiceLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieving all products")]
    public static partial void LogRetrievingAllProducts(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieving product {ProductId}")]
    public static partial void LogRetrievingProduct(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating product with name {ProductName} and price {Price}")]
    public static partial void LogCreatingProduct(ILogger logger, string productName, decimal price);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} created successfully")]
    public static partial void LogProductCreatedSuccessfully(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create product {ProductId} due to timeout")]
    public static partial void LogProductCreationTimeout(ILogger logger, Exception ex, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating product {ProductId} with name {ProductName}")]
    public static partial void LogUpdatingProduct(ILogger logger, string productId, string productName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} updated successfully")]
    public static partial void LogProductUpdatedSuccessfully(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Product {ProductId} not found for update")]
    public static partial void LogProductNotFoundForUpdate(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting product {ProductId}")]
    public static partial void LogDeletingProduct(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} deleted successfully")]
    public static partial void LogProductDeletedSuccessfully(ILogger logger, string productId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Product {ProductId} not found for deletion")]
    public static partial void LogProductNotFoundForDeletion(ILogger logger, string productId);
}
