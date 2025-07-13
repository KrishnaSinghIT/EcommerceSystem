namespace Ecommerce.Application.Interface.CommonPersitance
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity); // need to look 
        IQueryable<T> Query();
    }
}
