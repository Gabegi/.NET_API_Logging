namespace LoggingProduction.Models;

public record CreateOrderRequest(
    string CustomerId,
    decimal Total);
