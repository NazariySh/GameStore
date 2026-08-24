using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Tests.TestData.Games;

public static class GenreTestData
{
    public static List<Genre> GetGenres()
    {
        return
        [
            new Genre
            {
               Id = Guid.Parse("1a550b8e-1ee3-4b46-8209-53f5a27d8c5a"),
               CategoryId = 1,
               Name = "Strategy",
            },
            new Genre
            {
                Id = Guid.Parse("8ed6b4f2-d12f-4d04-bfbb-5a2470cb8c68"),
                Name = "RTS",
                ParentGenreId = Guid.Parse("1a550b8e-1ee3-4b46-8209-53f5a27d8c5a"),
            },
            new Genre
            {
                Id = Guid.Parse("3cb1d1c8-2070-4d84-92db-9aed3bf15b0e"),
                Name = "TBS",
                ParentGenreId = Guid.Parse("1a550b8e-1ee3-4b46-8209-53f5a27d8c5a"),
            },

            new Genre
            {
                Id = Guid.Parse("fe13b7d1-3a89-4d8c-9f83-5a63e056cae6"),
                Name = "RPG",
            },

            new Genre
            {
                Id = Guid.Parse("1778cda1-ff57-44c1-a551-c3e8fc4edb2b"),
                Name = "Sports",
            },

            new Genre
            {
                Id = Guid.Parse("70f1f908-14b7-4441-8be1-d3879cdfc41e"),
                Name = "Races",
            },
            new Genre
            {
                Id = Guid.Parse("64c65702-b3cb-40c3-810a-9c334dfe1b6f"),
                Name = "Rally",
                ParentGenreId = Guid.Parse("70f1f908-14b7-4441-8be1-d3879cdfc41e"),
            },
            new Genre
            {
                Id = Guid.Parse("f6be7bbc-367e-49b9-9d81-07c5041ac899"),
                Name = "Arcade",
                ParentGenreId = Guid.Parse("70f1f908-14b7-4441-8be1-d3879cdfc41e"),
            },
            new Genre
            {
                Id = Guid.Parse("83cef2d7-cabb-4023-8fca-9f2f8ad2f7d7"),
                Name = "Formula",
                ParentGenreId = Guid.Parse("70f1f908-14b7-4441-8be1-d3879cdfc41e"),
            },
            new Genre
            {
                Id = Guid.Parse("e866c4db-a2e6-4cdd-a82c-8db9ad6be1e2"),
                Name = "Off-road",
                ParentGenreId = Guid.Parse("70f1f908-14b7-4441-8be1-d3879cdfc41e"),
            },

            new Genre
            {
                Id = Guid.Parse("fd314fdc-f2a9-45bb-9227-b70b849d1610"),
                Name = "Action",
            },
            new Genre
            {
                Id = Guid.Parse("5a382f9a-863b-455f-abcb-8aef71e8deef"),
                Name = "FPS",
                ParentGenreId = Guid.Parse("fd314fdc-f2a9-45bb-9227-b70b849d1610"),
            },
            new Genre
            {
                Id = Guid.Parse("f0d6290e-fb5e-43dc-9a84-2cbc935bf902"),
                Name = "TPS",
                ParentGenreId = Guid.Parse("fd314fdc-f2a9-45bb-9227-b70b849d1610"),
            },

            new Genre
            {
                Id = Guid.Parse("b74cb1ff-ef8f-4321-843e-1d5a33e573a1"),
                Name = "Adventure",
            },
            new Genre
            {
                Id = Guid.Parse("1e9ba6fa-70c2-4137-89e0-0413207580ec"),
                Name = "Puzzle & Skill",
            },
       ];
    }

    public static List<Genre> GetGenres(ICollection<Guid> ids)
    {
        return GetGenres().Where(g => ids.Contains(g.Id)).ToList();
    }

    public static Genre GetGenre()
    {
        return GetGenres()[0];
    }

    public static Genre GetGenreById(Guid id)
    {
        return GetGenres().FirstOrDefault(g => g.Id == id)
               ?? throw new InvalidOperationException($"Genre with ID {id} not found for testing.");
    }

    public static Genre GetGenreByName(string name)
    {
        return GetGenres().FirstOrDefault(g => g.Name.Equals(name))
               ?? throw new InvalidOperationException($"No genre found with name '{name}' in test data.");
    }

    public static Genre GetGenreWithSubGenres()
    {
        return new Genre
        {
            Id = Guid.Parse("1a550b8e-1ee3-4b46-8209-53f5a27d8c5a"),
            Name = "Strategy",
            SubGenres =
            [
                new Genre
                {
                    Id = Guid.Parse("8ed6b4f2-d12f-4d04-bfbb-5a2470cb8c68"),
                    Name = "RTS",
                },
                new Genre
                {
                    Id = Guid.Parse("3cb1d1c8-2070-4d84-92db-9aed3bf15b0e"),
                    Name = "TBS",
                },
            ],
        };
    }

    public static CreateGenreRequest GetCreateRequest()
    {
        return new CreateGenreRequest
        {
            Genre = new GenreCreateDto
            {
                Name = "Survival",
            },
        };
    }

    public static CreateGenreRequest GetCreateRequestWithParentGenre()
    {
        return new CreateGenreRequest
        {
            Genre = new GenreCreateDto
            {
                Name = "Survival",
                ParentGenreId = new EntityId(GetGenreByName("Adventure").Id),
            },
        };
    }

    public static CreateGenreRequest GetInvalidCreateRequest()
    {
        return new CreateGenreRequest
        {
            Genre = new GenreCreateDto
            {
                Name = "Invalid name",
                ParentGenreId = null,
            },
        };
    }

    public static UpdateGenreRequest GetUpdateRequest(string? id = null)
    {
        return new UpdateGenreRequest
        {
            Genre = new GenreUpdateDto
            {
                Id = !string.IsNullOrEmpty(id) ? EntityId.Parse(id) : new EntityId(Guid.NewGuid()),
                Name = "Updated Genre",
                ParentGenreId = new EntityId(GetGenreByName("Adventure").Id),
            },
        };
    }

    public static UpdateGenreRequest GetInvalidUpdateRequest(Guid? id = null)
    {
        return new UpdateGenreRequest
        {
            Genre = new GenreUpdateDto
            {
                Id = new EntityId(id ?? Guid.NewGuid()),
                Name = "Invalid name",
                ParentGenreId = new EntityId(),
            },
        };
    }
}