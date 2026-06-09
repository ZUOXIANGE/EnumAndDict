using System.Text.Json;
using System.Text.Json.Serialization;
using FastEnumCore = FastEnumUtility.FastEnum;

namespace EnumDictDemo.Models.Dto;

public class EnumStringConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetInt32().ToString(),
            _ => null
        };

        if (value == null)
            throw new JsonException($"Cannot parse enum {typeof(TEnum).Name}");

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            return result;

        if (int.TryParse(value, out var intValue))
        {
            var enumValue = (TEnum)(object)intValue;
            if (FastEnumCore.IsDefined(enumValue))
                return enumValue;
        }

        var validValues = string.Join(", ", FastEnumCore.GetNames<TEnum>());
        throw new JsonException($"Invalid value '{value}' for enum {typeof(TEnum).Name}. Valid values: {validValues}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}