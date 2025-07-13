using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);

            return result.IsSuccess ? CreatedAtAction(nameof(GetOrderById), new { id = result.Data }, result) : BadRequest(result);
        }

        /// <summary>
        /// Gets order details by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, status);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Gets all orders for a customer.
        /// </summary>
        [HttpGet("/api/customers/{customerId:int}/orders")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var result = await _orderService.GetOrdersByCustomerAsync(customerId);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
