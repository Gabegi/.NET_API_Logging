namespace LoggingShared.Requests;

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
