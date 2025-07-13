using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<List<ProductDto>>> GetAllProductsAsync();
        Task<Result<string>> UpdateInventoryAsync(int productId, int quantityToAdd);
    }
}
