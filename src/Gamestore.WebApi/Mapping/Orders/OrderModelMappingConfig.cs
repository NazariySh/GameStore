using System.Globalization;
using Gamestore.BLL.DTOs.Orders;
using Gamestore.WebApi.Models.Orders;
using Mapster;

namespace Gamestore.WebApi.Mapping.Orders;

public class OrderModelMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderDto, OrderModel>()
            .Map(
                dest => dest.Id,
                src => src.Id != Guid.Empty ? src.Id.ToString() : src.MongoOrderId.ToString())
            .Map(
                dest => dest.CustomerId,
                src => src.CustomerId != Guid.Empty ? src.CustomerId.ToString() : src.MongoCustomerId ?? string.Empty);

        config.NewConfig<OrderGameDto, OrderGameModel>()
            .Map(
                dest => dest.ProductId,
                src => src.ProductId != Guid.Empty ? src.ProductId.ToString() : src.MongoProductId.ToString());

        config.NewConfig<OrderQueryModel, OrderQueryDto>()
            .Map(dest => dest.Start, src => ParseDateTime(src.Start))
            .Map(dest => dest.End, src => ParseDateTime(src.End));
    }

    private static DateTime? ParseDateTime(string? dateTimeString)
    {
        if (string.IsNullOrEmpty(dateTimeString))
        {
            return null;
        }

        var normalizedDateString = dateTimeString.Trim();

        var indexOfGmt = normalizedDateString.IndexOf("GMT", StringComparison.Ordinal);

        if (indexOfGmt > 0)
        {
            normalizedDateString = normalizedDateString[..indexOfGmt].Trim();
        }

        return DateTime.TryParse(normalizedDateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dateTime)
            ? dateTime
            : throw new ArgumentException($"Invalid date format: '{dateTimeString}'");
    }
}