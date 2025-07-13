using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Application.Observers;
using Ecommerce.Application.Services.Implementations;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.Tests.UnitTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow = new();
        private readonly Mock<IOrderStatusNotifier> _mockNotifier = new();
        private readonly Mock<ILogger<OrderService>> _mockLogger = new();

        private OrderService CreateService()
        => new(_mockUow.Object, _mockNotifier.Object, _mockLogger.Object);

        [Fact]
        public async Task Should_Apply_Premium_Discount_When_Customer_Is_Premium()
        {
            // Arrange
            var service = CreateService();
            var customer = new Customer { Id = 1, CustomerType = CustomerType.Premium, Name = "John", Email = "john@example.com" };
            var product = new Product { Id = 1, Name = "Book", Price = 100, ProductType = ProductType.Digital };

            _mockUow.Setup(u => u.Customers.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockUow.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
            _mockUow.Setup(u => u.Orders.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var request = new CreateOrderRequest
            {
                CustomerId = 1,
                Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, Quantity = 1 }
            }
            };

            // Act
            var result = await service.CreateOrderAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _mockUow.Verify(u => u.Orders.AddAsync(It.Is<Order>(o => o.TotalAmount == 90)), Times.Once); // 10% discount
        }

        [Fact]
        public void Should_Build_Order_Using_Builder()
        {
            // Arrange
            var order = new OrderBuilder()
                .WithCustomerId(123)
                .WithTotalAmount(200)
                .WithStatus(Ecommerce.Domain.Enums.OrderStatus.Pending)
                .Build();

            // Act & Assert
            Assert.Equal(123, order.CustomerId);
            Assert.Equal(200, order.TotalAmount);
            Assert.Equal(Ecommerce.Domain.Enums.OrderStatus.Pending, order.Status);
        }
    }
}
