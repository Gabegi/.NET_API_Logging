using LoggingShared.Models;

namespace LoggingMVP.Repository;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<Order> CreateAsync(Order order);
    Task<IEnumerable<Order>> SearchAsync(string? customerId = null);
}
