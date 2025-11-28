namespace LoggingProduction.Models;

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
