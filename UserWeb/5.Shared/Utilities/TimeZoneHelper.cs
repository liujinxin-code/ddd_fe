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

        public static DateTimeOffset UtcToZone(DateTimeOffset utc, string tzId)
        {
            var zone = GetZone(tzId);
            return TimeZoneInfo.ConvertTime(utc, zone);
        }

        public static DateTimeOffset NowInZone(string tzId)
            => UtcToZone(DateTimeOffset.UtcNow, tzId);

        private static TimeZoneInfo GetZone(string tzId)
        {
            return Cache.GetOrAdd(tzId, TimeZoneInfo.FindSystemTimeZoneById);
        }

        /// <summary>
        /// 安全解析时区：复用缓存；若 ID 非法 / 系统缺少 tzdata（如 Alpine）解析失败，兜底返回 UTC，保证调用方不挂。默认返回Asia/Shanghai时区
        /// 全项目唯一的时区解析入口（HTTP 中间件与业务转换统一走这里）。
        /// </summary>
        public static TimeZoneInfo GetZoneSafe(string tzId = "Asia/Shanghai")
        {
            try
            {
                return Cache.GetOrAdd(tzId, TimeZoneInfo.FindSystemTimeZoneById);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
