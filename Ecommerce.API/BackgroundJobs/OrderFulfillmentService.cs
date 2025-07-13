using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Domain.Enums;

namespace Ecommerce.API.BackgroundJobs
{
    public class OrderFulfillmentService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderFulfillmentService> _logger;

        public OrderFulfillmentService(IServiceProvider serviceProvider, ILogger<OrderFulfillmentService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Fulfillment Service started at {Time}", DateTime.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var processingOrders = unitOfWork.Orders.Query()
                        .Where(o => o.Status == OrderStatus.Processing)
                        .ToList();

                    if (!processingOrders.Any())
                    {
                        _logger.LogInformation("No orders in 'Processing' state at {Time}", DateTime.Now);
                    }

                    foreach (var order in processingOrders)
                    {
                        _logger.LogInformation($"Processing Order #{order.Id}");

                        // Simulate fulfillment delay
                        await Task.Delay(2000);

                        order.Status = OrderStatus.Shipped; // or Delivered
                        _logger.LogInformation("Order #{OrderId} marked as 'Shipped'", order.Id);
                    }

                    await unitOfWork.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing orders");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // run every 15 sec
            }
        }
    }
}
