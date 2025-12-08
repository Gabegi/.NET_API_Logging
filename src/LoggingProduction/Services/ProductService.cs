using LoggingProduction.Data.Repositories;
using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;
using LoggingProduction.Infrastructure.Telemetry;

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

        ProductServiceLogger.LogRetrievingAllProducts(_logger);
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("GetProductById");
        activity?.SetTag("product.id", id);

        ProductServiceLogger.LogRetrievingProduct(_logger, id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity("CreateProduct");
        activity?.SetTag("product.name", request.Name);
        activity?.SetTag("product.price", request.Price);

        var productId = Guid.NewGuid().ToString();

        ProductServiceLogger.LogCreatingProduct(_logger, request.Name, request.Price);

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
                ProductServiceLogger.LogProductCreatedSuccessfully(_logger, created.Id);
                return created;
            }
        }
        catch (TimeoutException ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            ProductServiceLogger.LogProductCreationTimeout(_logger, ex, productId);
            throw;
        }
    }

    public async Task<Product?> UpdateProductAsync(string id, UpdateProductRequest request)
    {
        ProductServiceLogger.LogUpdatingProduct(_logger, id, request.Name);

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
            ProductServiceLogger.LogProductUpdatedSuccessfully(_logger, id);
        }
        else
        {
            ProductServiceLogger.LogProductNotFoundForUpdate(_logger, id);
        }

        return updated;
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        ProductServiceLogger.LogDeletingProduct(_logger, id);
        var deleted = await _repository.DeleteAsync(id);

        if (deleted)
        {
            ProductServiceLogger.LogProductDeletedSuccessfully(_logger, id);
        }
        else
        {
            ProductServiceLogger.LogProductNotFoundForDeletion(_logger, id);
        }

        return deleted;
    }
}
