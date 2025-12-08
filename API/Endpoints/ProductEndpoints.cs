using LoggingProduction.Services;
using LoggingProduction.Data.Models.DTOs;

namespace LoggingProduction.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", async (IProductService service) =>
            Results.Ok(await service.GetAllProductsAsync()))
            .WithName("GetAllProducts")
            .WithOpenApi();

        group.MapGet("/{id}", async (string id, IProductService service) =>
        {
            var product = await service.GetProductByIdAsync(id);
            return product is null
                ? Results.NotFound(new { message = "Product not found" })
                : Results.Ok(product);
        })
            .WithName("GetProductById")
            .WithOpenApi();

        group.MapPost("/", async (CreateProductRequest request, IProductService service) =>
        {
            try
            {
                var product = await service.CreateProductAsync(request);
                return Results.Created($"/api/products/{product.Id}", product);
            }
            catch (TimeoutException)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        })
            .WithName("CreateProduct")
            .WithOpenApi();

        group.MapPut("/{id}", async (string id, UpdateProductRequest request, IProductService service) =>
        {
            var product = await service.UpdateProductAsync(id, request);
            return product is null
                ? Results.NotFound(new { message = "Product not found" })
                : Results.Ok(product);
        })
            .WithName("UpdateProduct")
            .WithOpenApi();

        group.MapDelete("/{id}", async (string id, IProductService service) =>
        {
            var deleted = await service.DeleteProductAsync(id);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { message = "Product not found" });
        })
            .WithName("DeleteProduct")
            .WithOpenApi();
    }
}
