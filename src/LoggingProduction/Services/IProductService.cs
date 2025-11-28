using LoggingProduction.Data.Models.DTOs;
using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(string id);
    Task<Product> CreateProductAsync(CreateProductRequest request);
    Task<Product?> UpdateProductAsync(string id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(string id);
}
