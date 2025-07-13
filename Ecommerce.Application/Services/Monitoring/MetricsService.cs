namespace Ecommerce.Application.Services.Monitoring
{
    public class MetricsService : IMetricsService
    {
        private int _ordersPlaced = 0;
        private int _failedOrders = 0;
        private int _inventoryUpdates = 0;

        public void IncrementOrdersPlaced() => Interlocked.Increment(ref _ordersPlaced);
        public void IncrementFailedOrders() => Interlocked.Increment(ref _failedOrders);
        public void IncrementInventoryUpdates() => Interlocked.Increment(ref _inventoryUpdates);

        public int GetOrdersPlaced() => _ordersPlaced;
        public int GetFailedOrders() => _failedOrders;
        public int GetInventoryUpdates() => _inventoryUpdates;
    }
}
