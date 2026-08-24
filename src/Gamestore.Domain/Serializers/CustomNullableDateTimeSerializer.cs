using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

#pragma warning disable IDE0010

namespace Gamestore.Domain.Serializers;

public class CustomNullableDateTimeSerializer : SerializerBase<DateTime?>
{
    public const string NullStringValue = CustomNullableStringSerializer.NullStringValue;

    public override DateTime? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        switch (bsonType)
        {
            case BsonType.Null:
                context.Reader.ReadNull();
                return null;

            case BsonType.String:
                var dateValue = context.Reader.ReadString();
                return string.IsNullOrWhiteSpace(dateValue) || dateValue.Equals(NullStringValue, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : DateTime.Parse(dateValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            case BsonType.DateTime:
                var millis = context.Reader.ReadDateTime();
                return DateTimeOffset.FromUnixTimeMilliseconds(millis).UtcDateTime;

            default:
                throw new FormatException($"Cannot deserialize BsonType '{bsonType}' to DateTime");
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime? value)
    {
        if (value.HasValue)
        {
            var utcMillis = new DateTimeOffset(value.Value.ToUniversalTime()).ToUnixTimeMilliseconds();
            context.Writer.WriteDateTime(utcMillis);
        }
        else
        {
            context.Writer.WriteNull();
        }
    }
}