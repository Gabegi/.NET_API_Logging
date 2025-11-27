namespace LoggingProduction.Requests;

public record PaymentRequest(
    string OrderId,
    decimal Amount,
    string PaymentMethod);
