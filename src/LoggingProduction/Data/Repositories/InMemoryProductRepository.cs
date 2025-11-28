using System.Collections.Concurrent;
using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Data.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ILogger<InMemoryProductRepository> _logger;
    private readonly ConcurrentDictionary<string, Product> _products = new();

    public InMemoryProductRepository(ILogger<InMemoryProductRepository> logger)
    {
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching product {ProductId} from repository", id);
        await Task.Delay(Random.Shared.Next(10, 50));

        var product = _products.GetValueOrDefault(id);

        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found in repository", id);
        }
        else
        {
            _logger.LogInformation("Product {ProductId} retrieved successfully", id);
        }

        return product;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _logger.LogInformation("Saving product {ProductId} to repository with name {ProductName} and price {Price}",
            product.Id, product.Name, product.Price);

        await Task.Delay(Random.Shared.Next(50, 200));

        if (Random.Shared.Next(100) < 5)
        {
            _logger.LogError("Database connection timeout while saving product {ProductId}", product.Id);
            throw new TimeoutException("Database connection timeout");
        }

        _products[product.Id] = product;
        _logger.LogInformation("Product {ProductId} saved successfully", product.Id);

        return product;
    }

    public async Task<Product?> UpdateAsync(string id, Product product)
    {
        _logger.LogInformation("Updating product {ProductId} with name {ProductName}", id, product.Name);

        await Task.Delay(Random.Shared.Next(50, 150));

        if (!_products.ContainsKey(id))
        {
            _logger.LogWarning("Product {ProductId} not found for update", id);
            return null;
        }

        var updated = product with { Id = id };
        _products[id] = updated;
        _logger.LogInformation("Product {ProductId} updated successfully", id);

        return updated;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        await Task.Delay(Random.Shared.Next(20, 100));

        var deleted = _products.TryRemove(id, out _);

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

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all products from repository");

        await Task.Delay(Random.Shared.Next(20, 100));

        var products = _products.Values.ToList();
        _logger.LogInformation("Retrieved {Count} products from repository", products.Count);

        return products;
    }
}
