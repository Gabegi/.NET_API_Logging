namespace LoggingProduction.Models.DTOs;

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
