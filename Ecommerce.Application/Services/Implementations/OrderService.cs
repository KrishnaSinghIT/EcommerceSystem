using Ecommerce.Application.BackgroundQueue;
using Ecommerce.Application.Common;
using Ecommerce.Application.Discounts;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Factories;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Observers;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Application.Services.Monitoring;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderStatusNotifier _notifier;
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderProcessingQueue _orderQueue;
        private readonly IMetricsService _metrics;

        public OrderService(IUnitOfWork unitOfWork,IOrderStatusNotifier notifier, ILogger<OrderService> logger, IOrderProcessingQueue orderQueue, IMetricsService metrics)
        {
            _unitOfWork = unitOfWork;
            _notifier = notifier;
            _logger = logger;
            _orderQueue = orderQueue;
            _metrics = metrics;
        }
        public async Task<Result<int>> CreateOrderAsync(CreateOrderRequest request)
        {
            _logger.LogInformation("Creating order for customer {CustomerId} at {Time}", request.CustomerId,DateTime.Now);
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);

            if (customer == null)
            {
                _metrics.IncrementFailedOrders(); 
                return Result<int>.Failure("Customer not found");
            }

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
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);

                if (product == null)
                {
                    _metrics.IncrementFailedOrders();
                    return Result<int>.Failure($"Product ID {item.ProductId} not found");
                }

                if (product.ProductType == ProductType.Physical && product.InventoryCount < item.Quantity)
                {
                    _metrics.IncrementFailedOrders(); 
                    return Result<int>.Failure($"Insufficient inventory for product {product.Name}");
                }

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
            
            _metrics.IncrementOrdersPlaced();

            _logger.LogInformation("Order Created succesfully for CustomerId {CustomerId} at {Time}", request.CustomerId, DateTime.Now);
            return Result<int>.Success(order.Id, "Order created successfully.");
        }
        public async Task<Result<OrderResponse>> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.Query()
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return Result<OrderResponse>.Failure("Order not found.");

            var response = new OrderResponse
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

            return Result<OrderResponse>.Success(response, "Order retrieved successfully.");
        }

        public async Task<Result<List<OrderResponse>>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _unitOfWork.Orders.Query()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ToListAsync();

            if (!orders.Any())
                return Result<List<OrderResponse>>.Failure("No orders found for this customer.");

            var orderResponses = orders.Select(order => new OrderResponse
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
            }).ToList();

            return Result<List<OrderResponse>>.Success(orderResponses, "Orders retrieved successfully.");

        }

        public async Task<Result<string>> UpdateOrderStatusAsync(int id, string status)
        {
            _logger.LogInformation("Updating order status for customer {CustomerId} at {Time}", id, DateTime.Now);
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return Result<string>.Failure("Order not found.");

            if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
                return Result<string>.Failure("Invalid order status.");

            // Enqueue only if status is Processing
            if (newStatus == OrderStatus.Processing)
            {
                _logger.LogInformation("Enqueuing order {OrderId} for fulfillment", id);
                _orderQueue.Enqueue(order.Id);
            }

            order.Status = newStatus;
            await _unitOfWork.SaveChangesAsync();

            _metrics.IncrementInventoryUpdates();

            _logger.LogInformation("Order status updated successfully for customer {CustomerId} at {Time}", id, DateTime.Now);
            await _notifier.NotifyAsync(order.Id, newStatus.ToString());

            return Result<string>.Success("Order status updated successfully.");
        }

    }
}
