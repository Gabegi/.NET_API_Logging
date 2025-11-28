using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Data.Repositories;

namespace LoggingProduction.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        _logger.LogInformation("Retrieving all orders");
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving order {OrderId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid().ToString();

        _logger.LogInformation("Creating order for customer {CustomerId} with total {Total}",
            request.CustomerId, request.Total);

        var order = new Order(
            orderId,
            request.CustomerId,
            request.Total,
            OrderStatus.Pending,
            DateTime.UtcNow);

        try
        {
            var created = await _repository.CreateAsync(order);
            _logger.LogInformation("Order {OrderId} created successfully with status {Status}",
                created.Id, created.Status);
            return created;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Failed to create order {OrderId} due to timeout", orderId);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> SearchOrdersAsync(string? customerId)
    {
        _logger.LogInformation("Searching orders with customerId filter: {CustomerId}",
            customerId ?? "all");

        return await _repository.SearchAsync(customerId);
    }
}
