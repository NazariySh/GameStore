using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Publishers;

namespace Gamestore.BLL.Interfaces.Games;

public interface IPublisherService
{
    Task<IReadOnlyList<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameDto>> GetPublisherGamesAsync(string companyName, CancellationToken cancellationToken = default);

    Task<PublisherDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PublisherDto> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);

    Task<CreatePublisherResponse> CreateAsync(CreatePublisherRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdatePublisherRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}