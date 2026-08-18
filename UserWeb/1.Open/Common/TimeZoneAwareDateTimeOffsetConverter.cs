using System.Text.Json;
using System.Text.Json.Serialization;

namespace Open.Common;

/// <summary>
/// 全局 DateTimeOffset 序列化转换器：写出时按 <see cref="TimeZoneContext.Current"/> 指定的请求时区，
/// 将 UTC / 源时区时间转换为该时区本地时间（保留正确偏移，如 +08:00 / -05:00），前端无需自行算时区。
/// 读出时原样解析（保留其瞬时，便于往返）。
///
/// 同时注册 <see cref="TimeZoneAwareDateTimeOffsetConverter"/>（非可空）与
/// <see cref="TimeZoneAwareNullableDateTimeOffsetConverter"/>（可空），覆盖 DeleteAuditor.DeleteTime 这类 DateTimeOffset? 字段。
///
/// 生效前提：DateTimeOffset 必须携带真实瞬时（本项目实体用 DateTimeOffset.Now / UtcNow，偏移真实），
/// 转换器按瞬时换算，不会算错。
/// </summary>
public sealed class TimeZoneAwareDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    /// 将给定瞬时按当前请求时区换算；未设置时区时原样返回（即按存储偏移输出）。
    /// </summary>
    internal static DateTimeOffset ToRequestZone(DateTimeOffset value)
    {
        var zone = TimeZoneContext.Current.Value;
        return zone is null ? value : TimeZoneInfo.ConvertTime(value, zone);
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
