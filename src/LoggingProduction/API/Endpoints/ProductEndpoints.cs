using LoggingProduction.API.Handlers;

namespace LoggingProduction.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var handler = app.Services.GetRequiredService<ProductHandler>();

        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", handler.GetAllProducts)
            .WithName("GetAllProducts")
            .WithOpenApi();

        group.MapGet("/{id}", handler.GetProductById)
            .WithName("GetProductById")
            .WithOpenApi();

        group.MapPost("/", handler.CreateProduct)
            .WithName("CreateProduct")
            .WithOpenApi();

        group.MapPut("/{id}", handler.UpdateProduct)
            .WithName("UpdateProduct")
            .WithOpenApi();

        group.MapDelete("/{id}", handler.DeleteProduct)
            .WithName("DeleteProduct")
            .WithOpenApi();
    }
}
