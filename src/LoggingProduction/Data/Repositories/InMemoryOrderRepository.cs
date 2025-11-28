using System.Collections.Concurrent;
using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Data.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ILogger<InMemoryOrderRepository> _logger;
    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public InMemoryOrderRepository(ILogger<InMemoryOrderRepository> logger)
    {
        _logger = logger;
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching order {OrderId} from repository", id);

        await Task.Delay(Random.Shared.Next(10, 50));

        var order = _orders.GetValueOrDefault(id);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found in repository", id);
        }
        else
        {
            _logger.LogInformation("Order {OrderId} retrieved successfully with status {Status}",
                id, order.Status);
        }

        return order;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _logger.LogInformation("Saving order {OrderId} to repository for customer {CustomerId} with total {Total}",
            order.Id, order.CustomerId, order.Total);

        await Task.Delay(Random.Shared.Next(50, 200));

        if (Random.Shared.Next(100) < 10)
        {
            _logger.LogError("Database connection timeout while saving order {OrderId}", order.Id);
            throw new TimeoutException("Database connection timeout");
        }

        _orders[order.Id] = order;
        _logger.LogInformation("Order {OrderId} saved successfully", order.Id);

        return order;
    }

    public async Task<IEnumerable<Order>> SearchAsync(string? customerId = null)
    {
        _logger.LogInformation("Searching orders with customerId filter: {CustomerId}", customerId ?? "all");

        await Task.Delay(Random.Shared.Next(20, 100));

        var results = string.IsNullOrEmpty(customerId)
            ? _orders.Values.ToList()
            : _orders.Values.Where(o => o.CustomerId == customerId).ToList();

        _logger.LogInformation("Search completed, found {Count} orders", results.Count);

        return results;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all orders from repository");

        await Task.Delay(Random.Shared.Next(20, 100));

        var orders = _orders.Values.ToList();
        _logger.LogInformation("Retrieved {Count} orders from repository", orders.Count);

        return orders;
    }
}
