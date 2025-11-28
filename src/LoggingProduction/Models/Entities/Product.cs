namespace LoggingProduction.Models.Entities;

public record Product(
    string Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);
