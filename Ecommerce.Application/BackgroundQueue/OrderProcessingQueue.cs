using System.Collections.Concurrent;

namespace Ecommerce.Application.BackgroundQueue
{
    public class OrderProcessingQueue : IOrderProcessingQueue
    {
        private readonly ConcurrentQueue<int> _queue = new();

        public void Enqueue(int orderId)
        {
            _queue.Enqueue(orderId);
        }

        public bool TryDequeue(out int orderId)
        {
            return _queue.TryDequeue(out orderId);
        }
    }
}
