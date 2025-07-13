namespace Ecommerce.Application.Observers
{
    public class EmailNotifier : IOrderStatusObserver
    {
        public Task OnOrderStatusChanged(int orderId, string newStatus)
        {
            Console.WriteLine($"[EMAIL] Order {orderId} status changed to {newStatus}");
            return Task.CompletedTask;
        }
    }
}
