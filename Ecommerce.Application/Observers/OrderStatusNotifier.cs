namespace Ecommerce.Application.Observers
{
    public class OrderStatusNotifier : IOrderStatusNotifier
    {
        private readonly List<IOrderStatusObserver> _observers = new();

        public void Subscribe(IOrderStatusObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IOrderStatusObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyAsync(int orderId, string newStatus)
        {
            foreach (var observer in _observers)
            {
                await observer.OnOrderStatusChanged(orderId, newStatus);
            }
        }
    }
}
