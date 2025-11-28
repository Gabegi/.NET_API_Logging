namespace LoggingProduction.Models.DTOs;

public record CreateOrderRequest(
    string CustomerId,
    decimal Total);
