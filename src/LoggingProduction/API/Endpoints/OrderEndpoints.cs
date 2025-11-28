using LoggingProduction.Services;

namespace LoggingProduction.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var service = app.Services.GetRequiredService<IOrderHandler>();

        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("/", service.GetAllOrders)
            .WithName("GetAllOrders")
            .WithOpenApi();

        group.MapGet("/{id}", service.GetOrderById)
            .WithName("GetOrderById")
            .WithOpenApi();

        group.MapPost("/", service.CreateOrder)
            .WithName("CreateOrder")
            .WithOpenApi();

        group.MapGet("/search", service.SearchOrders)
            .WithName("SearchOrders")
            .WithOpenApi();
    }
}
