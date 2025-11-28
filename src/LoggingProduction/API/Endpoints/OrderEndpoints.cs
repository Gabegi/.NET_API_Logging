using LoggingProduction.API.Handlers;

namespace LoggingProduction.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var handler = app.Services.GetRequiredService<IOrderHandler>();

        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("/", handler.GetAllOrders)
            .WithName("GetAllOrders")
            .WithOpenApi();

        group.MapGet("/{id}", handler.GetOrderById)
            .WithName("GetOrderById")
            .WithOpenApi();

        group.MapPost("/", handler.CreateOrder)
            .WithName("CreateOrder")
            .WithOpenApi();

        group.MapGet("/search", handler.SearchOrders)
            .WithName("SearchOrders")
            .WithOpenApi();
    }
}
