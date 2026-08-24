using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Tests.TestData.Games;

public static class PlatformTestData
{
    public static List<Platform> GetPlatforms()
    {
        return
        [
            new Platform
            {
                Id = Guid.Parse("d2c0ee9f-1d39-4c12-8106-71a86045e6e4"),
                Type = "Mobile",
            },
            new Platform
            {
                Id = Guid.Parse("f448a541-a3c3-49d8-bdd7-821e91afa5d8"),
                Type = "Browser",
            },
            new Platform
            {
                Id = Guid.Parse("b9ef7c87-b60d-4f06-8797-56e96743fdc1"),
                Type = "Desktop",
            },
            new Platform
            {
                Id = Guid.Parse("68e29181-8a50-4be1-bd6a-1946ac4ab1c8"),
                Type = "Console",
            },
        ];
    }

    public static List<Platform> GetPlatforms(ICollection<Guid> ids)
    {
        return GetPlatforms().Where(p => ids.Contains(p.Id)).ToList();
    }

    public static Platform GetPlatform()
    {
        return GetPlatforms()[0];
    }

    public static Platform GetPlatformByType(string type)
    {
        return GetPlatforms().FirstOrDefault(p => p.Type.Equals(type))
               ?? throw new InvalidOperationException($"No platform found with type '{type}' in test data.");
    }

    public static CreatePlatformRequest GetCreateRequest()
    {
        return new CreatePlatformRequest
        {
            Platform = new PlatformCreateDto
            {
                Type = "PC",
            },
        };
    }

    public static CreatePlatformRequest GetInvalidCreateRequest()
    {
        return new CreatePlatformRequest
        {
            Platform = new PlatformCreateDto
            {
                Type = "Invalid type##",
            },
        };
    }

    public static UpdatePlatformRequest GetUpdateRequest(Guid? id = null)
    {
        return new UpdatePlatformRequest
        {
            Platform = new PlatformUpdateDto
            {
                Id = id ?? Guid.NewGuid(),
                Type = "New type",
            },
        };
    }

    public static UpdatePlatformRequest GetInvalidUpdateRequest(Guid? id = null)
    {
        return new UpdatePlatformRequest
        {
            Platform = new PlatformUpdateDto
            {
                Type = "Invalid type##",
                Id = id ?? Guid.NewGuid(),
            },
        };
    }
}