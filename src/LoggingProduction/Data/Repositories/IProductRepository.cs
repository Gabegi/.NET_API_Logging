using LoggingProduction.Data.Models.Entities;

namespace LoggingProduction.Data.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(string id, Product product);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<Product>> GetAllAsync();
}
