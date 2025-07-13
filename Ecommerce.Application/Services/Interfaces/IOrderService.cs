using Ecommerce.Application.DTOs.Order;

namespace Ecommerce.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderResponse>> GetOrdersByCustomerAsync(int customerId);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
    }
}
