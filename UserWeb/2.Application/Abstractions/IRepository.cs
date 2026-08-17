using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 实现不要进行AsNoTracking()，保证实体被EF追踪状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<T?> GetByIdAsync(long id, CancellationToken ct = default);

        /// <summary>
        /// 进行AsNoTracking()，查询专用，保证实体被EF追踪状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<T?> GetByIdAsNoTrackingAsync(long id, CancellationToken ct = default);
        Task<bool> AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity, CancellationToken ct = default);
    }
}
