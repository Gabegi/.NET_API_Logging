namespace LoggingShared.Models;

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
