using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

#pragma warning disable IDE0010

namespace Gamestore.Domain.Serializers;

public class PercentageSerializer : SerializerBase<int?>
{
    public override int? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        switch (bsonType)
        {
            case BsonType.Null:
                context.Reader.ReadNull();
                return null;

            case BsonType.Double:
                var value = context.Reader.ReadDouble();
                return (int)Math.Round(value * 100, MidpointRounding.AwayFromZero);

            case BsonType.Int32:
                return context.Reader.ReadInt32();

            case BsonType.Int64:
                return (int)context.Reader.ReadInt64();

            default:
                throw new FormatException($"Cannot deserialize BsonType '{bsonType}' to percentage integer.");
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int? value)
    {
        if (value.HasValue)
        {
            context.Writer.WriteDouble(value.Value / 100.0);
        }
        else
        {
            context.Writer.WriteNull();
        }
    }
}