namespace LoggingShared.Requests;

public record PaymentRequest(
    string OrderId,
    decimal Amount,
    string PaymentMethod);
