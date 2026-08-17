using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>
        /// 执行一个完整业务操作，并在遇到乐观并发冲突时自动重试。
        /// 每次重试前会清除 DbContext 中已被跟踪的实体，使下一次执行能从数据库重新加载最新数据并重放业务逻辑，
        /// 从而避免余额等核心字段的丢更新。所有重试均失败后，抛出 <see cref="FlowEngine.Application.Common.Exceptions.ConcurrencyConflictException"/>。
        /// </summary>
        /// <param name="work">包含「加载实体 - 修改 - 记录流水 - SaveChanges」的完整业务操作。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <param name="maxRetries">最大尝试次数（含首次），默认 3 次。</param>
        Task ExecuteWithRetryAsync(Func<Task> work, CancellationToken cancellationToken = default, int maxRetries = 3);

        /// <summary>
        /// 同上，但业务操作带有返回值。
        /// </summary>
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken = default, int maxRetries = 3);
    }
}
