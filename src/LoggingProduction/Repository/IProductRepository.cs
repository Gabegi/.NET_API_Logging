using LoggingShared.Models;

namespace LoggingProduction.Repository;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(string id, Product product);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<Product>> GetAllAsync();
}
