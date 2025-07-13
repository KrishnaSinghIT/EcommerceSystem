using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Order;

namespace Ecommerce.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<int>> CreateOrderAsync(CreateOrderRequest request);
        Task<Result<OrderResponse>> GetOrderByIdAsync(int id);
        Task<Result<List<OrderResponse>>> GetOrdersByCustomerAsync(int customerId);
        Task<Result<string>> UpdateOrderStatusAsync(int id, string status);
    }
}
