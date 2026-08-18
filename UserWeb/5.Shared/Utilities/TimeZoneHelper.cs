using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public static class TimeZoneHelper
    {
        // 缓存时区对象，避免反复 FindSystemTimeZoneById
        private static readonly ConcurrentDictionary<string, TimeZoneInfo> Cache = new();

        /// <summary>
        /// 安全解析时区：复用缓存；若 ID 非法 / 系统缺少 tzdata（如 Alpine）解析失败，兜底返回 UTC，保证调用方不挂。默认返回Asia/Shanghai时区
        /// 全项目唯一的时区解析入口（HTTP 中间件与业务转换统一走这里）。
        /// </summary>
        public static TimeZoneInfo GetZoneSafe(string tzId = "Asia/Shanghai")
        {
            try
            {
                return Cache.GetOrAdd(tzId, TimeZoneInfo.FindSystemTimeZoneById(tzId));
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
        /// <summary>
        /// 把传入时间的"墙上时钟"(DateTime) 当作上海本地时间，换算为目标时区。
        /// 前置条件：传入值须为上海本地时间（数据库统一以 DateTimeOffset.Now 存上海时区）。
        /// ⚠️ 若服务器本地时区非 Asia/Shanghai，存储值偏移不符，换算结果会偏差。
        /// </summary>
        /// <param name="time">数据库存储的 DateTimeOffset（上海时区）。</param>
        /// <param name="otherTzId">目标时区 ID（请求时区）。</param>
        /// <returns>换算到目标时区后的时间。</returns>
        public static DateTimeOffset ConvertShanghaiToOther(DateTimeOffset time, string otherTzId)
        {
            TimeZoneInfo shanghaiZone = GetZoneSafe();
            TimeZoneInfo otherZone = GetZoneSafe(otherTzId);
            var shanghaiLocal = DateTime.SpecifyKind(
       time.DateTime,
       DateTimeKind.Unspecified);

            var shanghaiOffset = new DateTimeOffset(
   shanghaiLocal,
     shanghaiZone.GetUtcOffset(time));

            return TimeZoneInfo.ConvertTime(shanghaiOffset, otherZone);
        }

        /// <summary>
        /// 其他时区转上海时区
        /// </summary>
        /// <param name="now"></param>
        /// <param name="toTzId"></param>
        /// <returns></returns>
        public static DateTimeOffset ConvertOtherToShanghai(DateTimeOffset time, string otherTzId)
        {
            TimeZoneInfo shanghaiZone = GetZoneSafe();
            TimeZoneInfo otherZone = GetZoneSafe(otherTzId);
            //转换成datetime时间
            var otherTime = DateTime.SpecifyKind(
       time.DateTime,
       DateTimeKind.Unspecified);

            var otherOffset = new DateTimeOffset(
   otherTime,
     otherZone.GetUtcOffset(time));
            return TimeZoneInfo.ConvertTime(otherOffset, shanghaiZone);
        }
    }
}
