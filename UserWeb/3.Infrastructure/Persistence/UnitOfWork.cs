using Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// 工作单元实现。
    /// 当前项目只有一个 DbContext，所以工作单元就是对 SaveChangesAsync 的封装。
    /// </summary>
    public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
    {
        /// <summary>提交当前 DbContext 中跟踪到的所有变更。</summary>
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return dbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// 执行完整业务操作，遇到乐观并发冲突时清除跟踪并整段重放，最多尝试 <paramref name="maxRetries"/> 次。
        /// 这与 tk_user.user_version 并发令牌配合，保证并发的「转账 / 下单」等操作不会互相覆盖余额。
        /// 若重试耗尽仍有冲突，抛出 <see cref="ConcurrencyConflictException"/>（而非底层 EF 异常），
        /// 使应用层无需依赖 EF Core。
        /// </summary>
        public async Task ExecuteWithRetryAsync(Func<Task> work, CancellationToken ct = default, int maxRetries = 3)
        {
            try
            {
                for (var attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        await work();
                        return;
                    }
                    catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                    {
                        // 并发冲突：放弃本次跟踪的修改，下一轮从数据库重新加载最新数据并重算。
                        dbContext.ChangeTracker.Clear();
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyConflictException(
                    "操作后提交时发生乐观并发冲突，且重试均已失败。", ex);
            }
        }

        /// <summary>
        /// 同上，但业务操作带有返回值。
        /// </summary>
        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> work, CancellationToken ct = default, int maxRetries = 3)
        {
            try
            {
                for (var attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        return await work();
                    }
                    catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                    {
                        dbContext.ChangeTracker.Clear();
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyConflictException(
                    "操作后提交时发生乐观并发冲突，且重试均已失败。", ex);
            }

            // 不可达：最后一次尝试若仍抛并发异常，会被外层 catch 转换为 ConcurrencyConflictException，
            // 不会走到这里。default! 仅为满足编译器返回路径。
            return default!;
        }
    }
}
