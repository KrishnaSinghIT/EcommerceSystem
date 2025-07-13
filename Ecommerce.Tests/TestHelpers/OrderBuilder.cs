using Ecommerce.Domain.Entities;

namespace Ecommerce.Tests.TestHelpers
{
    public class OrderBuilder
    {
        private readonly Order _order = new();

        public OrderBuilder WithCustomerId(int id)
        {
            _order.CustomerId = id;
            return this;
        }

        public OrderBuilder WithTotalAmount(decimal amount)
        {
            _order.TotalAmount = amount;
            return this;
        }
        public OrderBuilder WithStatus(Ecommerce.Domain.Enums.OrderStatus status)
        {
            _order.Status = status;
            return this;
        }
        public Order Build() => _order;
    }
}
