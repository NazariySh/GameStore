using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

#pragma warning disable IDE0010

namespace Gamestore.Domain.Serializers;

public class CustomNullableStringSerializer : SerializerBase<string?>
{
    public const string NullStringValue = "NULL";

    public override string? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        switch (bsonType)
        {
            case BsonType.Null:
                context.Reader.ReadNull();
                return null;

            case BsonType.String:
                var value = context.Reader.ReadString();
                return string.IsNullOrWhiteSpace(value) || value.Equals(NullStringValue, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : value;

            case BsonType.Int32:
                return context.Reader.ReadInt32().ToString();

            case BsonType.Int64:
                return context.Reader.ReadInt64().ToString();

            case BsonType.Double:
                return context.Reader.ReadDouble().ToString(CultureInfo.InvariantCulture);

            default:
                throw new FormatException($"Cannot deserialize BsonType '{bsonType}' to string");
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string? value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
        }
        else
        {
            context.Writer.WriteString(value);
        }
    }
}