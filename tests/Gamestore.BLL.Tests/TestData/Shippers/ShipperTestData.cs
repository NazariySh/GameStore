using Gamestore.Domain.Entities.Shippers;

namespace Gamestore.BLL.Tests.TestData.Shippers;

public static class ShipperTestData
{
    public static List<Shipper> GetShippers()
    {
        return
        [
            new Shipper
            {
                ShipperId = 1,
                CompanyName = "Speedy Express",
                Phone = "(503) 555-9831",
            },
            new Shipper
            {
                ShipperId = 2,
                CompanyName = "United Package",
                Phone = "(503) 555-3199",
            },
            new Shipper
            {
                ShipperId = 3,
                CompanyName = "Federal Shipping",
                Phone = "(503) 555-9931",
            },
        ];
    }
}