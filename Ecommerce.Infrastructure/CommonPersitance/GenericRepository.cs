using Ecommerce.Application.Interface.CommonPersitance;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.CommonPersitance
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public virtual Task AddAsync(T entity)
            => _dbSet.AddAsync(entity).AsTask();

        public virtual void Update(T entity)
            => _dbSet.Update(entity);

        public virtual void Delete(T entity)
            => _dbSet.Remove(entity);

        public virtual IQueryable<T> Query()
            => _dbSet.AsQueryable();
    }
}
