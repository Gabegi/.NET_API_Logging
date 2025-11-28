using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Data.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<Order> CreateAsync(Order order);
    Task<IEnumerable<Order>> SearchAsync(string? customerId = null);
    Task<IEnumerable<Order>> GetAllAsync();
}
