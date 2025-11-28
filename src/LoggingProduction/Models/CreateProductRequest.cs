namespace LoggingProduction.Models;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
