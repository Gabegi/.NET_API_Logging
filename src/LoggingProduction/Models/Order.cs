namespace LoggingProduction.Models;

public record Product(
    string Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);

public record Order(
    string Id,
    string CustomerId,
    decimal Total,
    OrderStatus Status,
    DateTime CreatedAt);

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
