using LoggingProduction.Data.Repositories;
using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Services;

public class ProductHandler : IProductHandler
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductHandler> _logger;

    public ProductHandler(IProductRepository repository, ILogger<ProductHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IResult> GetAllProducts()
    {
        _logger.LogInformation("Retrieving all products");

        var products = await _repository.GetAllAsync();

        return Results.Ok(products);
    }

    public async Task<IResult> GetProductById(string id)
    {
        _logger.LogInformation("Retrieving product {ProductId}", id);

        var product = await _repository.GetByIdAsync(id);

        if (product is null)
        {
            _logger.LogWarning("Product {ProductId} not found", id);
            return Results.NotFound(new { message = "Product not found" });
        }

        return Results.Ok(product);
    }

    public async Task<IResult> CreateProduct(CreateProductRequest request)
    {
        var productId = Guid.NewGuid().ToString();

        _logger.LogInformation("Creating product with name {ProductName} and price {Price}",
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
            var created = await _repository.CreateAsync(product);
            _logger.LogInformation("Product {ProductId} created successfully", created.Id);

            return Results.Created($"/api/products/{created.Id}", created);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Failed to create product {ProductId} due to timeout", productId);
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    public async Task<IResult> UpdateProduct(string id, UpdateProductRequest request)
    {
        _logger.LogInformation("Updating product {ProductId} with name {ProductName}", id, request.Name);

        var product = new Product(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            DateTime.UtcNow);

        var updated = await _repository.UpdateAsync(id, product);

        if (updated is null)
        {
            _logger.LogWarning("Product {ProductId} not found for update", id);
            return Results.NotFound(new { message = "Product not found" });
        }

        _logger.LogInformation("Product {ProductId} updated successfully", id);
        return Results.Ok(updated);
    }

    public async Task<IResult> DeleteProduct(string id)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Product {ProductId} not found for deletion", id);
            return Results.NotFound(new { message = "Product not found" });
        }

        _logger.LogInformation("Product {ProductId} deleted successfully", id);
        return Results.NoContent();
    }
}
