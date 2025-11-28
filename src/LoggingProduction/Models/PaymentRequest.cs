namespace LoggingProduction.Models;

public record PaymentRequest(
    string OrderId,
    decimal Amount,
    string PaymentMethod);
