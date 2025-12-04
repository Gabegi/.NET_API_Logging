using LoggingProduction.Data.Repositories;
using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Telemetry;

namespace LoggingProduction.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("GetAllProducts");
        activity?.SetTag("operation.type", "read");

        _logger.LogInformation("Retrieving all products");
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("GetProductById");
        activity?.SetTag("product.id", id);

        _logger.LogInformation("Retrieving product {ProductId}", id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("CreateProduct");
        activity?.SetTag("product.name", request.Name);
        activity?.SetTag("product.price", request.Price);

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
            // Create child span for repository operation
            using (var repoActivity = ActivitySourceProvider.Source.StartActivity("SaveProductToRepository"))
            {
                repoActivity?.SetTag("product.id", productId);
                var created = await _repository.CreateAsync(product);
                repoActivity?.SetTag("repository.success", true);
                _logger.LogInformation("Product {ProductId} created successfully", created.Id);
                return created;
            }
        }
        catch (TimeoutException ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            _logger.LogError(ex, "Failed to create product {ProductId} due to timeout", productId);
            throw;
        }
    }

    public async Task<Product?> UpdateProductAsync(string id, UpdateProductRequest request)
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

        if (updated is not null)
        {
            _logger.LogInformation("Product {ProductId} updated successfully", id);
        }
        else
        {
            _logger.LogWarning("Product {ProductId} not found for update", id);
        }

        return updated;
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);
        var deleted = await _repository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("Product {ProductId} deleted successfully", id);
        }
        else
        {
            _logger.LogWarning("Product {ProductId} not found for deletion", id);
        }

        return deleted;
    }
}
