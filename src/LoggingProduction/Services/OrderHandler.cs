using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Data.Repositories;

namespace LoggingProduction.Services;

public class OrderHandler : IOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderHandler> _logger;

    public OrderHandler(IOrderRepository repository, ILogger<OrderHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IResult> GetAllOrders()
    {
        _logger.LogInformation("Retrieving all orders");

        var orders = await _repository.GetAllAsync();

        return Results.Ok(orders);
    }

    public async Task<IResult> GetOrderById(string id)
    {
        _logger.LogInformation("Retrieving order {OrderId}", id);

        var order = await _repository.GetByIdAsync(id);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return Results.NotFound(new { message = "Order not found" });
        }

        return Results.Ok(order);
    }

    public async Task<IResult> CreateOrder(CreateOrderRequest request)
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

            return Results.Created($"/api/orders/{created.Id}", created);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Failed to create order {OrderId} due to timeout", orderId);
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    public async Task<IResult> SearchOrders(string? customerId)
    {
        _logger.LogInformation("Searching orders with customerId filter: {CustomerId}",
            customerId ?? "all");

        var orders = await _repository.SearchAsync(customerId);

        return Results.Ok(orders);
    }
}
