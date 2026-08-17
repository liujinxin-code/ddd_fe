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
        : IRequestHandler<GetHomepageNoticesQuery, ApiResult<List<NoticeResponse>>>
    {
        public async Task<ApiResult<List<NoticeResponse>>> Handle(GetHomepageNoticesQuery query, CancellationToken ct)
        {
            var (notices, total) = await noticeRepository.GetHomepageNoticesAsync(query.PageIndex, query.PageSize, ct);

            if (notices.Count == 0)
            {
                return ApiResult<List<NoticeResponse>>.Successed(new List<NoticeResponse>(), 0);
            }

            var items = notices.Select(n => new NoticeResponse
            {
                NoticeId = n.NoticeId,
                NoticeContent = n.NoticeContent,
                NoticeType = n.NoticeType,
                CreateTime = n.CreateTime
            }).ToList();

            // data 为 List（IList），ApiResult.Successed 会按 list.Count 回填 DataTotal，
            // 故此处显式构造，保留真实总条数 total 供前端分页（页码/页大小已在请求中，无需回显）。
            return new ApiResult<List<NoticeResponse>>
            {
                Code = 200,
                Message = "Success!",
                Data = items,
                DataTotal = total
            };
        }
    }
}
