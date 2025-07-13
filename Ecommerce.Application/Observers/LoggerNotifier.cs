namespace Ecommerce.Application.Observers
{
    public class LoggerNotifier : IOrderStatusObserver
    {
        public Task OnOrderStatusChanged(int orderId, string newStatus)
        {
            Console.WriteLine($"[LOG] Order {orderId} changed to {newStatus}");
            return Task.CompletedTask;
        }
    }
}
