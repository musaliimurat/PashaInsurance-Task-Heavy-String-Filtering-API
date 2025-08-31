using PashaInsuranceFiltering.SharedKernel.Application.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Infrastructure.Persistence.InMemory
{
    public abstract class BaseInMemoryRepository<T, TId> : IRepositoryBase<T, TId> where T : class
    {
        protected readonly ConcurrentDictionary<TId, T> Store = new();
        protected abstract TId GetId(T entity);
        public Task AddAsync(T entity, CancellationToken ct = default)
        {
            Store[GetId(entity)] = entity;
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync(TId id, CancellationToken ct = default)
        {
            Store.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            Store[GetId(entity)] = entity;
            return Task.CompletedTask;
        }
    }
}
