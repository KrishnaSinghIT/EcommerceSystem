namespace Ecommerce.Application.DTOs.Order
{
    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
