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
            var orderId = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { orderId });
        }

        /// <summary>
        /// Gets order details by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            // Will be implemented later (get logic)
            return Ok(new { message = "Not implemented yet" });
        }

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            // Will be implemented later (status update logic)
            return Ok(new { message = "Not implemented yet" });
        }

        /// <summary>
        /// Gets all orders for a customer.
        /// </summary>
        [HttpGet("/api/customers/{customerId:int}/orders")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            // Will be implemented later (filter by customer)
            return Ok(new { message = "Not implemented yet" });
        }
    }
}
