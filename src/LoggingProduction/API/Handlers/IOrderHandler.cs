using LoggingProduction.Data.Models.DTOs;

namespace LoggingProduction.API.Handlers;

public interface IOrderHandler
{
    Task<IResult> GetAllOrders();
    Task<IResult> GetOrderById(string id);
    Task<IResult> CreateOrder(CreateOrderRequest request);
    Task<IResult> SearchOrders(string? customerId);
}
