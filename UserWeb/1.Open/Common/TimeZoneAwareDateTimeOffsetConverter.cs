using Shared.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Open.Common;

/// <summary>
/// 全局 DateTimeOffset 序列化转换器：数据库统一以上海时区（Asia/Shanghai）存储（实体用 DateTimeOffset.Now），
/// 写出时按 <see cref="TimeZoneContext.Current"/> 指定的请求时区，调用 TimeZoneHelper.CovertShanghaiToOther
/// 把上海时间换算为目标时区本地时间（保留正确偏移，如 +08:00 / -05:00），前端无需自行算时区。
/// 读出时原样解析（保留其瞬时，便于往返）。
///
/// 同时注册 <see cref="TimeZoneAwareDateTimeOffsetConverter"/>（非可空）与
/// <see cref="TimeZoneAwareNullableDateTimeOffsetConverter"/>（可空），覆盖 DeleteAuditor.DeleteTime 这类 DateTimeOffset? 字段。
///
/// 生效前提：存储值须为上海本地时间（即服务器本地时区须为 Asia/Shanghai，或由写入侧显式以上海时区构造）；
/// CovertShanghaiToOther 会把传入值的"墙上时钟"当作上海时间换算，若存储偏移不符则结果偏差。
/// </summary>
public sealed class TimeZoneAwareDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    /// 把数据库存储的上海时区时间换算为当前请求时区；未设置时区时原样返回（即上海时间）。
    /// </summary>
    internal static DateTimeOffset ToRequestZone(DateTimeOffset value)
    {
        var zone = TimeZoneContext.Current.Value;
        return zone is null ? value : TimeZoneHelper.ConvertShanghaiToOther(value, zone);
    }

    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => reader.GetDateTimeOffset();

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(ToRequestZone(value));
}

/// <summary>
/// 可空 DateTimeOffset 的对应转换器（逻辑同上，仅多处理 null）。
/// </summary>
public sealed class TimeZoneAwareNullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTimeOffset();

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(TimeZoneAwareDateTimeOffsetConverter.ToRequestZone(value.Value));
    }
}
