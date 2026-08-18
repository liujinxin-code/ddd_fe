using Application.Abstractions.Repositories;
using Application.Common.Models;

using Application.Features.Notice.Models;
using Application.Features.Notice;
using Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Notice
{
    public class GetHomepageNoticesQueryHandler(INoticeRepository noticeRepository)
        : IRequestHandler<GetHomepageNoticesQuery, PagedResult<NoticeResponse>>
    {
        public async Task<PagedResult<NoticeResponse>> Handle(GetHomepageNoticesQuery query, CancellationToken ct)
        {
            var (notices, total) = await noticeRepository.GetHomepageNoticesAsync(query.PageIndex, query.PageSize, ct);

            if (notices.Count == 0)
            {
                return new PagedResult<NoticeResponse>(new List<NoticeResponse>(), 0);
            }

            var items = notices.Select(n => new NoticeResponse
            {
                NoticeId = n.NoticeId,
                NoticeContent = n.NoticeContent,
                NoticeType = n.NoticeType,
                CreateTime = n.CreateTime
            }).ToList();

            // 返回中性分页载体 PagedResult，真实总条数 total 由 TotalCount 携带；
            // HTTP 边缘层（ApiPaged）会显式构造信封，避免 IList 被 ApiResult.Successed 按 Count 覆盖总条数。
            return new PagedResult<NoticeResponse>(items, total);
        }
    }
}
