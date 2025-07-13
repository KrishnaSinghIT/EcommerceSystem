using Ecommerce.Application.DTOs.Order;

namespace Ecommerce.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(CreateOrderRequest request);
    }
}
