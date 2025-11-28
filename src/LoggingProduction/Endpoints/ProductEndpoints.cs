using LoggingProduction.Repository;
using LoggingProduction.Models.DTOs;
using LoggingProduction.Models.Entities;

namespace LoggingProduction.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetAllProducts)
            .WithName("GetAllProducts")
            .WithOpenApi();

        group.MapGet("/{id}", GetProductById)
            .WithName("GetProductById")
            .WithOpenApi();

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithOpenApi();

        group.MapPut("/{id}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithOpenApi();

        group.MapDelete("/{id}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllProducts(
        IProductRepository repository,
        ILogger<Program> logger)
    {
        logger.LogInformation("Retrieving all products");

        var products = await repository.GetAllAsync();

        return Results.Ok(products);
    }

    private static async Task<IResult> GetProductById(
        string id,
        IProductRepository repository,
        ILogger<Program> logger)
    {
        logger.LogInformation("Retrieving product {ProductId}", id);

        var product = await repository.GetByIdAsync(id);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} not found", id);
            return Results.NotFound(new { message = "Product not found" });
        }

        return Results.Ok(product);
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        IProductRepository repository,
        ILogger<Program> logger)
    {
        var productId = Guid.NewGuid().ToString();

        logger.LogInformation("Creating product with name {ProductName} and price {Price}",
            request.Name, request.Price);

        var product = new Product(
            productId,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            DateTime.UtcNow);

        try
        {
            var created = await repository.CreateAsync(product);
            logger.LogInformation("Product {ProductId} created successfully", created.Id);

            return Results.Created($"/api/products/{created.Id}", created);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Failed to create product {ProductId} due to timeout", productId);
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> UpdateProduct(
        string id,
        UpdateProductRequest request,
        IProductRepository repository,
        ILogger<Program> logger)
    {
        logger.LogInformation("Updating product {ProductId} with name {ProductName}", id, request.Name);

        var product = new Product(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            DateTime.UtcNow);

        var updated = await repository.UpdateAsync(id, product);

        if (updated is null)
        {
            logger.LogWarning("Product {ProductId} not found for update", id);
            return Results.NotFound(new { message = "Product not found" });
        }

        logger.LogInformation("Product {ProductId} updated successfully", id);
        return Results.Ok(updated);
    }

    private static async Task<IResult> DeleteProduct(
        string id,
        IProductRepository repository,
        ILogger<Program> logger)
    {
        logger.LogInformation("Deleting product {ProductId}", id);

        var deleted = await repository.DeleteAsync(id);

        if (!deleted)
        {
            logger.LogWarning("Product {ProductId} not found for deletion", id);
            return Results.NotFound(new { message = "Product not found" });
        }

        logger.LogInformation("Product {ProductId} deleted successfully", id);
        return Results.NoContent();
    }
}
