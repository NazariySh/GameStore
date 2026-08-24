using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Payments;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class PaymentMethodDataSeeder : EntityDataSeeder<PaymentMethod>
{
    public PaymentMethodDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<PaymentMethodDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
    }

    public override int Order => 1;

    protected override string FileName => "payment-methods";
}