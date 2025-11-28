using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderByIdAsync(string id);
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
    Task<IEnumerable<Order>> SearchOrdersAsync(string? customerId);
}
