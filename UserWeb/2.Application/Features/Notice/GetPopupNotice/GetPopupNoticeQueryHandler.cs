using Application.Abstractions.Repositories;
using Application.Common.Models;
using Application.Features.Notice.Models;
using Application.Features.Notice;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Notice
{
    public class GetPopupNoticeQueryHandler(INoticeRepository noticeRepository)
        : IRequestHandler<GetPopupNoticeQuery, ApiResult<NoticeResponse>>
    {
        public async Task<ApiResult<NoticeResponse>> Handle(GetPopupNoticeQuery query, CancellationToken ct)
        {
            var notice = await noticeRepository.GetPopupNoticeAsync(ct);
            if (notice is null)
            {
                // 弹窗公告可能未配置：返回成功且 Data=null，前端据此不弹窗。
                return ApiResult<NoticeResponse>.Successed(null!);
            }

            var item = new NoticeResponse
            {
                NoticeId = notice.NoticeId,
                NoticeContent = notice.NoticeContent,
                NoticeType = notice.NoticeType,
                CreateTime = notice.CreateTime
            };

            return ApiResult<NoticeResponse>.Successed(item);
        }
    }
}
