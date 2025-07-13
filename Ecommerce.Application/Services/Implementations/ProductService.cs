using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ProductDto>>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();

            if (products == null || !products.Any())
                return Result<List<ProductDto>>.Failure("No products found.");

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ProductType = p.ProductType.ToString(),
                InventoryCount = p.InventoryCount
            }).ToList();
            return Result<List<ProductDto>>.Success(productDtos, "Products retrieved successfully.");
        }

        public async Task<Result<string>> UpdateInventoryAsync(int productId, int quantityToAdd)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);

            if (product == null)
                return Result<string>.Failure("Product not found.");

            if (product.ProductType == ProductType.Digital)
                return Result<string>.Failure("Cannot update inventory for digital products.");

            product.InventoryCount += quantityToAdd;
            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success("Inventory updated successfully.");
        }

    }
}
