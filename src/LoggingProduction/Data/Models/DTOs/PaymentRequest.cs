namespace LoggingProduction.Data.Models.DTOs;

public record PaymentRequest(
    string OrderId,
    decimal Amount,
    string PaymentMethod);
