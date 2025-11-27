namespace LoggingProduction.Requests;

public record CreateOrderRequest(
    string CustomerId,
    decimal Total);
