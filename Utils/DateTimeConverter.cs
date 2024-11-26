using System.Text.Json;
using System.Text.Json.Serialization;

namespace PatientDataApp.Utils;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Převod na UTC při čtení
        var dateTime = reader.GetDateTime();
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Zajištění, že výstup je vždy v UTC
        writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
    }
}