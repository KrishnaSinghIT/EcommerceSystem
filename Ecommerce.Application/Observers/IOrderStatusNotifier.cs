namespace Ecommerce.Application.Observers
{
    public interface IOrderStatusNotifier
    {
        void Subscribe(IOrderStatusObserver observer);
        void Unsubscribe(IOrderStatusObserver observer);
        Task NotifyAsync(int orderId, string newStatus);
    }
}
