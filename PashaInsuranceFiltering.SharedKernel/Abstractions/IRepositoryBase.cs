using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Application.Abstractions
{
    public interface IRepositoryBase<T, TId> where T : class
    {
        Task AddAsync(T entity, CancellationToken ct = default);
        Task<T?> GetAsync(TId id, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
    }
}
