using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }
    }
}
