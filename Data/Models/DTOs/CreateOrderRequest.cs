namespace LoggingProduction.Data.Models.DTOs;

public record CreateOrderRequest(
    string CustomerId,
    decimal Total);
