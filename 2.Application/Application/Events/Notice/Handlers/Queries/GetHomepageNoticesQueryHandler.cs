using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Common.Models.Notice;
using Application.Events.Notice.Contracts.Queries;
using Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Events.Notice.Handlers.Queries
{
    public class GetHomepageNoticesQueryHandler(INoticeRepository noticeRepository)
        : IRequestHandler<GetHomepageNoticesQuery, ApiResult<List<NoticeListItem>>>
    {
        public async Task<ApiResult<List<NoticeListItem>>> Handle(GetHomepageNoticesQuery query, CancellationToken ct)
        {
            var (notices, total) = await noticeRepository.GetHomepageNoticesAsync(query.PageIndex, query.PageSize, ct);

            if (notices.Count == 0)
            {
                return ApiResult<List<NoticeListItem>>.Successed(new List<NoticeListItem>(), 0);
            }

            var items = notices.Select(n => new NoticeListItem
            {
                NoticeId = n.NoticeId,
                NoticeContent = n.NoticeContent,
                NoticeType = n.NoticeType,
                CreateTime = n.CreateTime
            }).ToList();

            // data 为 List（IList），ApiResult.Successed 会按 list.Count 回填 DataTotal，
            // 故此处显式构造，保留真实总条数 total 供前端分页（页码/页大小已在请求中，无需回显）。
            return new ApiResult<List<NoticeListItem>>
            {
                Code = 200,
                Message = "Success!",
                Data = items,
                DataTotal = total
            };
        }
    }
}
