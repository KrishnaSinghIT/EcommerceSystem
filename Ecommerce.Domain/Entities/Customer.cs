using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public CustomerType CustomerType { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
