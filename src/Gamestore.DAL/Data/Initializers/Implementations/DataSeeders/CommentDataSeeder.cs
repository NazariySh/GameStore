using Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Microsoft.Extensions.Logging;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders;

public class CommentDataSeeder : EntityDataSeeder<Comment>
{
    public CommentDataSeeder(
        IUnitOfWork unitOfWork,
        ISeedDataExtractor dataExtractor,
        ILogger<CommentDataSeeder> logger)
        : base(unitOfWork, dataExtractor, logger)
    {
    }

    public override int Order => 3;

    protected override string FileName => "comments";
}