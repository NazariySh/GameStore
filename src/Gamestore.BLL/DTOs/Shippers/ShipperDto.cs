namespace Gamestore.BLL.DTOs.Shippers;

public record ShipperDto
{
    public int ShipperId { get; init; }

    public string CompanyName { get; init; }

    public string Phone { get; init; }
}