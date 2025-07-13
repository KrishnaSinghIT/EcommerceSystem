using Ecommerce.Application.Discounts;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Factories;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> CreateOrderAsync(CreateOrderRequest request)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId)
                ?? throw new Exception("Customer not found");

            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                Items = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId)
                    ?? throw new Exception($"Product ID {item.ProductId} not found");

                if (product.ProductType == ProductType.Physical && product.InventoryCount < item.Quantity)
                    throw new Exception($"Insufficient inventory for product {product.Name}");

                totalAmount += product.Price * item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });

                // Decrease inventory if physical
                if (product.ProductType == ProductType.Physical)
                    product.InventoryCount -= item.Quantity;

                // optional i will look into this later 
                var processor = ProductTypeProcessorFactory.Create(product.ProductType);
                await processor.ProcessAsync();
            }

            // Apply Discount Strategy
            IDiscountStrategy discountStrategy = customer.CustomerType switch
            {
                CustomerType.Premium => new PremiumCustomerDiscount(),
                CustomerType.Regular => new RegularCustomerDiscount(),
                _ => throw new Exception("Unknown customer type")
            };

            var discount = discountStrategy.CalculateDiscount(totalAmount);
            order.TotalAmount = totalAmount - discount;

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order.Id;
        }
    }
}
