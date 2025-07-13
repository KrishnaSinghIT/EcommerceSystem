using Ecommerce.Application.Discounts;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Factories;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
        public async Task<OrderResponse?> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.Query()
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            return new OrderResponse
            {
                OrderId = order.Id,
                CustomerName = order.Customer.Name,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _unitOfWork.Orders.Query()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToListAsync();

            return orders.Select(order => new OrderResponse
            {
                OrderId = order.Id,
                CustomerName = order.Customer.Name, // Or fetch from joined Customer
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            });
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null) return false;

            if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
                throw new Exception("Invalid status");

            order.Status = newStatus;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
