using Ecommerce.Application.BackgroundQueue;
using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Domain.Enums;

namespace Ecommerce.API.BackgroundJobs
{
    public class OrderFulfillmentService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderFulfillmentService> _logger;
        private readonly IOrderProcessingQueue _queue;
        public OrderFulfillmentService(IServiceProvider serviceProvider, ILogger<OrderFulfillmentService> logger, IOrderProcessingQueue queue)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _queue = queue;
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation("Order Fulfillment Service started at {Time}", DateTime.Now);

        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            using var scope = _serviceProvider.CreateScope();
        //            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //            var processingOrders = unitOfWork.Orders.Query()
        //                .Where(o => o.Status == OrderStatus.Processing)
        //                .ToList();

        //            if (!processingOrders.Any())
        //            {
        //                _logger.LogInformation("No orders in 'Processing' state at {Time}", DateTime.Now);
        //            }

        //            foreach (var order in processingOrders)
        //            {
        //                _logger.LogInformation($"Processing Order #{order.Id}");

        //                // Simulate fulfillment delay
        //                await Task.Delay(2000);

        //                order.Status = OrderStatus.Shipped; // or Delivered
        //                _logger.LogInformation("Order #{OrderId} marked as 'Shipped'", order.Id);
        //            }

        //            await unitOfWork.SaveChangesAsync(stoppingToken);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error occurred while processing orders");
        //        }

        //        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // run every 15 sec
        //    }
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Fulfillment Worker Started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out int orderId))
                {
                    int retryCount = 0;
                    bool success = false;

                    while (!success && retryCount < 3)
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            var order = await unitOfWork.Orders.GetByIdAsync(orderId);

                            if (order == null)
                            {
                                _logger.LogWarning("Order {OrderId} not found.", orderId);
                                break;
                            }

                            if (order.Status != OrderStatus.Processing)
                            {
                                _logger.LogInformation("Order {OrderId} already processed.", orderId);
                                break;
                            }

                            // Simulate processing delay
                            await Task.Delay(1500, stoppingToken);

                            order.Status = OrderStatus.Shipped;
                            await unitOfWork.SaveChangesAsync();

                            _logger.LogInformation("Order {OrderId} marked as 'Shipped'", orderId);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            _logger.LogWarning(ex, "Retry {RetryCount}/3 failed for Order {OrderId}", retryCount, orderId);
                            await Task.Delay(2000); 
                        }
                    }

                    if (!success)
                    {
                        _logger.LogError("❌ Order {OrderId} failed after 3 retries. Moving to dead-letter log.", orderId);
                    }
                }

                await Task.Delay(1000, stoppingToken); // short wait between queue checks
            }
        }
    }
}
