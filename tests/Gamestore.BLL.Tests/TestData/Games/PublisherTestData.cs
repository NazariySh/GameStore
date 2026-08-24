using Gamestore.BLL.DTOs.Games.Publishers;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Tests.TestData.Games;

public static class PublisherTestData
{
    public static List<Publisher> GetPublishers()
    {
        return
        [
            new Publisher
            {
                Id = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190001"),
                SupplierId = 1,
                CompanyName = "CD Projekt",
                HomePage = "https://www.cdprojekt.com",
                Description = "Polish game developer and publisher of Cyberpunk 2077.",
            },
            new Publisher
            {
                Id = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190002"),
                CompanyName = "FromSoftware",
                HomePage = null,
                Description = null,
            },
            new Publisher
            {
                Id = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190003"),
                CompanyName = "Electronic Arts",
                HomePage = "https://www.ea.com",
                Description = "Publisher of the FIFA series and other major sports titles.",
            },
            new Publisher
            {
                Id = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190004"),
                CompanyName = "Nintendo",
                HomePage = "https://www.nintendo.com",
                Description = "Japanese entertainment company and publisher of Mario Kart.",
            },
            new Publisher
            {
                Id = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190005"),
                CompanyName = "Team Cherry",
                HomePage = "https://www.teamcherry.com.au",
                Description = "Indie developer and publisher of Hollow Knight.",
            },
        ];
    }

    public static Publisher GetPublisher()
    {
        return GetPublishers()[0];
    }

    public static Publisher GetPublisherById(Guid id)
    {
        return GetPublishers().FirstOrDefault(p => p.Id == id)
            ?? throw new InvalidOperationException($"Publisher with ID {id} not found.");
    }

    public static CreatePublisherRequest GetCreateRequest()
    {
        return new CreatePublisherRequest
        {
            Publisher = new PublisherCreateDto
            {
                CompanyName = "Devolver Digital",
                HomePage = "https://www.devolverdigital.com",
                Description = "An independent game publisher known for creative and unconventional titles.",
            },
        };
    }

    public static CreatePublisherRequest GetInvalidCreateRequest()
    {
        return new CreatePublisherRequest
        {
            Publisher = new PublisherCreateDto
            {
                CompanyName = string.Empty,
            },
        };
    }

    public static UpdatePublisherRequest GetUpdateRequest(string? id = null)
    {
        return new UpdatePublisherRequest
        {
            Publisher = new PublisherUpdateDto
            {
                Id = !string.IsNullOrEmpty(id) ? EntityId.Parse(id) : new EntityId(Guid.NewGuid()),
                CompanyName = "Annapurna Interactive",
                HomePage = "https://www.annapurnainteractive.com",
                Description = "A publisher focused on artistic and story-driven indie games.",
            },
        };
    }

    public static UpdatePublisherRequest GetInvalidUpdateRequest(Guid? id = null)
    {
        return new UpdatePublisherRequest
        {
            Publisher = new PublisherUpdateDto
            {
                Id = new EntityId(id ?? Guid.NewGuid()),
                CompanyName = string.Empty,
            },
        };
    }
}