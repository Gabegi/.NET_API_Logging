using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Data.Repositories;
using LoggingProduction.Infrastructure.Telemetry;

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
        using var activity = ActivitySourceProvider.Source.StartActivity("GetAllOrders");
        activity?.SetTag("operation.type", "read");

        OrderServiceLogger.LogRetrievingAllOrders(_logger);
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(string id)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("GetOrderById");
        activity?.SetTag("order.id", id);

        OrderServiceLogger.LogRetrievingOrder(_logger, id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("CreateOrder");
        activity?.SetTag("customer.id", request.CustomerId);
        activity?.SetTag("order.total", request.Total);

        var orderId = Guid.NewGuid().ToString();

        OrderServiceLogger.LogCreatingOrder(_logger, request.CustomerId, request.Total);

        var order = new Order(
            orderId,
            request.CustomerId,
            request.Total,
            OrderStatus.Pending,
            DateTime.UtcNow);

        try
        {
            // Create child span for repository operation
            using (var repoActivity = ActivitySourceProvider.Source.StartActivity("SaveOrderToRepository"))
            {
                repoActivity?.SetTag("order.id", orderId);
                var created = await _repository.CreateAsync(order);
                repoActivity?.SetTag("repository.success", true);
                OrderServiceLogger.LogOrderCreatedSuccessfully(_logger, created.Id, created.Status.ToString());
                return created;
            }
        }
        catch (TimeoutException ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            OrderServiceLogger.LogOrderCreationTimeout(_logger, ex, orderId);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> SearchOrdersAsync(string? customerId)
    {
        OrderServiceLogger.LogSearchingOrders(_logger, customerId ?? "all");

        return await _repository.SearchAsync(customerId);
    }
}
