using System.Collections.Concurrent;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Telemetry;

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
        OrderRepositoryLogger.LogFetchingOrder(_logger, id);

        await Task.Delay(Random.Shared.Next(10, 50));

        var order = _orders.GetValueOrDefault(id);

        if (order == null)
        {
            OrderRepositoryLogger.LogOrderNotFound(_logger, id);
        }
        else
        {
            OrderRepositoryLogger.LogOrderFound(_logger, id);
        }

        return order;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        OrderRepositoryLogger.LogSavingOrder(_logger, order.Id, order.CustomerId, order.Total);

        await Task.Delay(Random.Shared.Next(50, 200));

        if (Random.Shared.Next(100) < 10)
        {
            OrderRepositoryLogger.LogDatabaseTimeout(_logger, new TimeoutException("Database connection timeout"), order.Id);
            throw new TimeoutException("Database connection timeout");
        }

        _orders[order.Id] = order;
        OrderRepositoryLogger.LogOrderSavedSuccessfully(_logger, order.Id);

        return order;
    }

    public async Task<IEnumerable<Order>> SearchAsync(string? customerId = null)
    {
        OrderRepositoryLogger.LogSearchingOrders(_logger, customerId ?? "all");

        await Task.Delay(Random.Shared.Next(20, 100));

        var results = string.IsNullOrEmpty(customerId)
            ? _orders.Values.ToList()
            : _orders.Values.Where(o => o.CustomerId == customerId).ToList();

        OrderRepositoryLogger.LogOrdersMatched(_logger, results.Count);

        return results;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        OrderRepositoryLogger.LogFetchingAllOrders(_logger);

        await Task.Delay(Random.Shared.Next(20, 100));

        var orders = _orders.Values.ToList();
        OrderRepositoryLogger.LogRetrievedOrders(_logger, orders.Count);

        return orders;
    }
}
