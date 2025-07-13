using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Gets all available products.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Updates product inventory.
        /// </summary>
        [HttpPost("{id}/inventory")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryRequest request)
        {
            var result = await _productService.UpdateInventoryAsync(id, request.QuantityToAdd);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Data });
        }


    }
}
