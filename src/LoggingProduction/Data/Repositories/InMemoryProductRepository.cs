using System.Collections.Concurrent;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Telemetry;

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
        ProductRepositoryLogger.LogFetchingProduct(_logger, id);
        await Task.Delay(Random.Shared.Next(10, 50));

        var product = _products.GetValueOrDefault(id);

        if (product == null)
        {
            ProductRepositoryLogger.LogProductNotFound(_logger, id);
        }
        else
        {
            ProductRepositoryLogger.LogProductFound(_logger, id);
        }

        return product;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        ProductRepositoryLogger.LogSavingProduct(_logger, product.Id, product.Name, product.Price);

        await Task.Delay(Random.Shared.Next(50, 200));

        if (Random.Shared.Next(100) < 5)
        {
            ProductRepositoryLogger.LogDatabaseTimeout(_logger, new TimeoutException("Database connection timeout"), product.Id);
            throw new TimeoutException("Database connection timeout");
        }

        _products[product.Id] = product;
        ProductRepositoryLogger.LogProductSavedSuccessfully(_logger, product.Id);

        return product;
    }

    public async Task<Product?> UpdateAsync(string id, Product product)
    {
        ProductRepositoryLogger.LogUpdatingProduct(_logger, id);

        await Task.Delay(Random.Shared.Next(50, 150));

        if (!_products.ContainsKey(id))
        {
            ProductRepositoryLogger.LogProductNotFound(_logger, id);
            return null;
        }

        var updated = product with { Id = id };
        _products[id] = updated;
        ProductRepositoryLogger.LogProductUpdated(_logger, id);

        return updated;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        ProductRepositoryLogger.LogDeletingProduct(_logger, id);

        await Task.Delay(Random.Shared.Next(20, 100));

        var deleted = _products.TryRemove(id, out _);

        if (deleted)
        {
            ProductRepositoryLogger.LogProductDeleted(_logger, id);
        }
        else
        {
            ProductRepositoryLogger.LogProductNotFound(_logger, id);
        }

        return deleted;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        ProductRepositoryLogger.LogFetchingAllProducts(_logger);

        await Task.Delay(Random.Shared.Next(20, 100));

        var products = _products.Values.ToList();
        ProductRepositoryLogger.LogRetrievedProducts(_logger, products.Count);

        return products;
    }
}
