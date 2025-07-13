using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public ProductType ProductType { get; set; }

        public int InventoryCount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
