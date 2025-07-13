namespace Ecommerce.Application.Services.Monitoring
{
    public interface IMetricsService
    {
        void IncrementOrdersPlaced();
        void IncrementFailedOrders();
        void IncrementInventoryUpdates();
        int GetOrdersPlaced();
        int GetFailedOrders();
        int GetInventoryUpdates();
    }
}
