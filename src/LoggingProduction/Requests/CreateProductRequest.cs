namespace LoggingProduction.Requests;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
