namespace Gamestore.DAL.Data.Initializers.Implementations.DataSeeders.DataExtractors;

public interface ISeedDataExtractor
{
    Task<List<T>> ExtractAsync<T>(string fileName, CancellationToken cancellationToken = default);
}