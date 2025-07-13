using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<List<ProductDto>>> GetAllProductsAsync()
        {
            const string cacheKey = "product_list_cache";

            if (_cache.TryGetValue(cacheKey, out List<ProductDto>? cachedProducts) && cachedProducts is not null)
            {
                _logger.LogInformation("Products served from cache.");
                return Result<List<ProductDto>>.Success(cachedProducts, "Products retrieved successfully.");
            }

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

            _logger.LogInformation("Products fetched from DB and cached.");

            _cache.Set(cacheKey, productDtos, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

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

            _cache.Remove("product_list_cache");

            return Result<string>.Success("Inventory updated successfully.");
        }

    }
}
