namespace Ecommerce.Application.Observers
{
    public interface IOrderStatusObserver
    {
        Task OnOrderStatusChanged(int orderId, string newStatus);
    }
}
