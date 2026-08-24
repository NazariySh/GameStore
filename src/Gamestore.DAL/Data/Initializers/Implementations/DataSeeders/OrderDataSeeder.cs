using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Orders;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class OrderDataSeeder : EntityDataSeeder<Order>
{
    public OrderDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<OrderDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
    }

    public override int Order => 3;

    protected override string FileName => "orders";
}