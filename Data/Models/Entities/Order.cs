namespace LoggingProduction.Data.Models.Entities;

public record Order(
    string Id,
    string CustomerId,
    decimal Total,
    OrderStatus Status,
    DateTime CreatedAt);
