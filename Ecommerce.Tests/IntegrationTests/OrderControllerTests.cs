using Ecommerce.Application.DTOs.Order;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Ecommerce.Tests.IntegrationTests
{
    public class OrderControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public OrderControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_CreateOrder_Returns_Created()
        {
            var request = new CreateOrderRequest
            {
                CustomerId = 1,
                Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, Quantity = 1 }
            }
            };

            var response = await _client.PostAsJsonAsync("/api/orders/createOrders", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

    }
}
