using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class PlatformDataSeeder : EntityDataSeeder<Platform>
{
    public PlatformDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<PlatformDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
    }

    public override int Order => 1;

    protected override string FileName => "platforms";
}