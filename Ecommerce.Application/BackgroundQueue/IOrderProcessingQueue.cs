namespace Ecommerce.Application.BackgroundQueue
{
    public interface IOrderProcessingQueue
    {
        void Enqueue(int orderId);
        bool TryDequeue(out int orderId);
    }
}
