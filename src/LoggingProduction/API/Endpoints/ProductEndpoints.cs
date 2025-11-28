using LoggingProduction.Services;

namespace LoggingProduction.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var service = app.Services.GetRequiredService<IProductHandler>();

        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", service.GetAllProducts)
            .WithName("GetAllProducts")
            .WithOpenApi();

        group.MapGet("/{id}", service.GetProductById)
            .WithName("GetProductById")
            .WithOpenApi();

        group.MapPost("/", service.CreateProduct)
            .WithName("CreateProduct")
            .WithOpenApi();

        group.MapPut("/{id}", service.UpdateProduct)
            .WithName("UpdateProduct")
            .WithOpenApi();

        group.MapDelete("/{id}", service.DeleteProduct)
            .WithName("DeleteProduct")
            .WithOpenApi();
    }
}
