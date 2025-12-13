using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        // Для поддержки изменений
        protected List<T> Data { get; set; }

        public InMemoryRepository(IEnumerable<T> data)
        {
            Data = data.ToList();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data.AsEnumerable());
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
        }

        // Добавление нового элемента
        public Task AddAsync(T entity)
        {
            Data.Add(entity);
            return Task.CompletedTask;
        }

        // Обновление существующего элемента
        public Task UpdateAsync(T entity)
        {
            var index = Data.FindIndex(x => x.Id == entity.Id);
            if (index >= 0)
            {
                Data[index] = entity;
            }
            return Task.CompletedTask;
        }

        // Удаление элемента
        public Task DeleteAsync(Guid id)
        {
            var entity = Data.FirstOrDefault(x => x.Id == id);
            if (entity != null)
            {
                Data.Remove(entity);
            }
            return Task.CompletedTask;
        }
    }
}