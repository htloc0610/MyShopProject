using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyShop.Helpers.Converters
{
    public sealed class NullableDateOnlyJsonConverter
    : JsonConverter<DateOnly?>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Unexpected token {reader.TokenType}");

            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateOnly.TryParseExact(
                    value,
                    Format,
                    out var date))
                return date;

            throw new JsonException($"Invalid DateOnly format: {value}");
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateOnly? value,
            JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(Format));
            else
                writer.WriteNullValue();
        }
    }

}
