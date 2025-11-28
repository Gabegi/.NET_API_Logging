using LoggingProduction.Services;
using LoggingProduction.Data.Models.DTOs;

namespace LoggingProduction.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("/", async (IOrderService service) =>
            Results.Ok(await service.GetAllOrdersAsync()))
            .WithName("GetAllOrders")
            .WithOpenApi();

        group.MapGet("/{id}", async (string id, IOrderService service) =>
        {
            var order = await service.GetOrderByIdAsync(id);
            return order is null
                ? Results.NotFound(new { message = "Order not found" })
                : Results.Ok(order);
        })
            .WithName("GetOrderById")
            .WithOpenApi();

        group.MapPost("/", async (CreateOrderRequest request, IOrderService service) =>
        {
            try
            {
                var order = await service.CreateOrderAsync(request);
                return Results.Created($"/api/orders/{order.Id}", order);
            }
            catch (TimeoutException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        })
            .WithName("CreateOrder")
            .WithOpenApi();

        group.MapGet("/search", async (string? customerId, IOrderService service) =>
            Results.Ok(await service.SearchOrdersAsync(customerId)))
            .WithName("SearchOrders")
            .WithOpenApi();
    }
}
