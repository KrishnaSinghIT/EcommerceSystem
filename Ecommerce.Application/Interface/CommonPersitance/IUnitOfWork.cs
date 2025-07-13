using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interface.CommonPersitance
{
    public interface IUnitOfWork
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
