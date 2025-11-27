namespace LoggingShared.Requests;

public record CreateOrderRequest(
    string CustomerId,
    decimal Total);
