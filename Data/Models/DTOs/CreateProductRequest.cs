namespace LoggingProduction.Data.Models.DTOs;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
