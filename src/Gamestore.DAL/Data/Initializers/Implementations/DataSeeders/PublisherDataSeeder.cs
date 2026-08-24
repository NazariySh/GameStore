using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class PublisherDataSeeder : EntityDataSeeder<Publisher>
{
    public PublisherDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<PublisherDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
    }

    public override int Order => 1;

    protected override string FileName => "publishers";
}