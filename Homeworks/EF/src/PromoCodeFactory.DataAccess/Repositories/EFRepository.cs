using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace PromoCodeFactory.DataAccess.Repositories
{
    public class EFRepository<T> : IRepository<T> where T: BaseEntity
    {
        private readonly DataContext __dbContext;
        public EFRepository(DataContext dbContext) { 
            __dbContext = dbContext;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return __dbContext.Set<T>().ToList();
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return __dbContext.Set<T>().FirstOrDefaultAsync<T>(x => x.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = __dbContext.Set<T>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public async Task<T> GetByIdWithIncludesAsync(Guid id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = __dbContext.Set<T>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}
