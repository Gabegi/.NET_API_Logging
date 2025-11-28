using LoggingProduction.Data.Models.DTOs;

namespace LoggingProduction.API.Handlers;

public interface IProductHandler
{
    Task<IResult> GetAllProducts();
    Task<IResult> GetProductById(string id);
    Task<IResult> CreateProduct(CreateProductRequest request);
    Task<IResult> UpdateProduct(string id, UpdateProductRequest request);
    Task<IResult> DeleteProduct(string id);
}
