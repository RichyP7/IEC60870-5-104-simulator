using System.Text.Json;
using System.Text.Json.Serialization;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.JsonConverter;

public class IecValueObjectConverter : JsonConverter<IecValueObject>
{
    public override IecValueObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        var rootElement = doc.RootElement;

        if (!rootElement.TryGetProperty("value", out var valueProp))
            throw new JsonException("Unknown IecValueObject type.");
        switch (valueProp.ValueKind)
        {
            case JsonValueKind.Number:
                return JsonSerializer.Deserialize<IecIntValueObject>(rootElement.GetRawText(), options);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return JsonSerializer.Deserialize<IecSinglePointValueObject>(rootElement.GetRawText(), options);
            case JsonValueKind.Object when rootElement.TryGetProperty("shortValue", out _):
                return JsonSerializer.Deserialize<IecValueScaledObject>(rootElement.GetRawText(), options);
            case JsonValueKind.Object:
                return JsonSerializer.Deserialize<IecDoublePointValueObject>(rootElement.GetRawText(), options);
            case JsonValueKind.Undefined:
                break;
            case JsonValueKind.Array:
                break;
            case JsonValueKind.String:
                break;
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        throw new JsonException("Unknown IecValueObject type.");
    }

    public override void Write(Utf8JsonWriter writer, IecValueObject value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}