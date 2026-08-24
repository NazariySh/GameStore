using System.Globalization;
using Gamestore.BLL.DTOs.Games;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Tests.TestData.Games;

public static class GameTestData
{
    public static List<Game> GetGames()
    {
        return
        [
            new Game
            {
                Id = Guid.Parse("11b07e1f-1cf2-4c0d-b0a2-d46fd3d74a11"),
                Name = "Star Command",
                Key = "star-command",
                Description = "A space-themed RTS strategy game.",
                Price = 39.99m,
                UnitInStock = 50,
                Discount = 5,
                ViewCount = 23,
                CommentCount = 3,
                CreatedAt = DateTime.Parse("2023-05-14T00:00:00Z", CultureInfo.InvariantCulture),
                CategoryId = 1,
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Strategy"),
                    GetGameGenreByName("RTS"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Mobile"),
                    GetGamePlatformByType("Desktop"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190001"),
                ImageUrl = "https://example.com/images/star-command.png",
            },
            new Game
            {
                Id = Guid.Parse("22954e2e-6659-4662-8d7c-9ac04f3a0ea6"),
                Name = "Formula Fury",
                Key = "formula-fury",
                Description = "Fast-paced formula racing game.",
                Price = 29.99m,
                UnitInStock = 75,
                Discount = 15,
                ViewCount = 247,
                CommentCount = 11,
                CreatedAt = DateTime.Parse("2022-11-20T00:00:00Z", CultureInfo.InvariantCulture),
                CategoryId = 1,
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Races"),
                    GetGameGenreByName("Formula"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Console"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190002"),
                ImageUrl = "https://example.com/images/formula-fury.png",
            },
            new Game
            {
                Id = Guid.Parse("39fc2c6d-f7dc-4cd7-85a0-79b3f62ff88e"),
                Name = "Fantasy Realms",
                Key = "fantasy-realms",
                Description = "An epic fantasy RPG with rich storytelling.",
                Price = 49.99m,
                UnitInStock = 60,
                Discount = 0,
                ViewCount = 36,
                CommentCount = 2,
                CreatedAt = DateTime.Parse("2021-08-05T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("RPG"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Desktop"),
                    GetGamePlatformByType("Mobile"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190003"),
                ImageUrl = "https://example.com/images/fantasy-realms.png",
            },
            new Game
            {
                Id = Guid.Parse("4f1c650a-883b-4b47-a989-9d3e8de54233"),
                Name = "Rally Racer X",
                Key = "rally-racer-x",
                Description = "Off-road rally racing simulation.",
                Price = 34.99m,
                UnitInStock = 80,
                Discount = 10,
                ViewCount = 58,
                CommentCount = 1,
                CreatedAt = DateTime.Parse("2022-02-11T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Races"),
                    GetGameGenreByName("Rally"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Browser"),
                    GetGamePlatformByType("Desktop"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190004"),
                ImageUrl = "https://example.com/images/rally-racer-x.png",
            },
            new Game
            {
                Id = Guid.Parse("53aa902d-111c-42c4-8136-65208d3b30d6"),
                Name = "Puzzle Box",
                Key = "puzzle-box",
                Description = "Mind-bending puzzle and skill game.",
                Price = 19.99m,
                UnitInStock = 40,
                Discount = 0,
                ViewCount = 19,
                CommentCount = 1,
                CreatedAt = DateTime.Parse("2023-07-08T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Puzzle & Skill"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Browser"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190005"),
            },
            new Game
            {
                Id = Guid.Parse("6158b497-e7a4-47e5-9c50-3a5d2a2c7b61"),
                Name = "Combat Arena",
                Key = "combat-arena",
                Description = "Intense multiplayer TPS action.",
                Price = 44.99m,
                UnitInStock = 70,
                Discount = 20,
                ViewCount = 85,
                CommentCount = 1,
                CreatedAt = DateTime.Parse("2020-09-25T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Action"),
                    GetGameGenreByName("TPS"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Console"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190001"),
            },
            new Game
            {
                Id = Guid.Parse("725d84fa-51a7-44a5-9466-8cfad1b56de4"),
                Name = "Fantasy Rally",
                Key = "fantasy-rally",
                Description = "Combines off-road rally and fantasy gameplay.",
                Price = 54.99m,
                UnitInStock = 65,
                Discount = 5,
                ViewCount = 64,
                CommentCount = 0,
                CreatedAt = DateTime.Parse("2021-12-17T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("RPG"),
                    GetGameGenreByName("Off-road"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Mobile"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190002"),
            },
            new Game
            {
                Id = Guid.Parse("84bc2cd6-6c29-4b4a-b8cb-7b20137aa1c9"),
                Name = "Sniper Hunt",
                Key = "sniper-hunt",
                Description = "Stealth-based FPS sniper game.",
                Price = 42.99m,
                UnitInStock = 90,
                Discount = 12,
                ViewCount = 29,
                CommentCount = 3,
                CreatedAt = DateTime.Parse("2023-01-19T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Action"),
                    GetGameGenreByName("FPS"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Console"),
                    GetGamePlatformByType("Desktop"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190003"),
                IsDeleted = true,
            },
            new Game
            {
                Id = Guid.Parse("9ed3a711-0e27-434b-bc87-3a48a93d276c"),
                Name = "Adventure Quest",
                Key = "adventure-quest",
                Description = "Classic point-and-click adventure game.",
                Price = 27.99m,
                UnitInStock = 45,
                Discount = 0,
                ViewCount = 13,
                CommentCount = 0,
                CreatedAt = DateTime.Parse("2022-04-06T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Adventure"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Browser"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190004"),
                IsDeleted = true,
            },
            new Game
            {
                Id = Guid.Parse("1f3b9c7b-5c99-4bcb-bb89-1a9b7d190005"),
                Name = "Tabletop War",
                Key = "tabletop-war",
                Description = "Turn-based strategy game with miniatures.",
                Price = 31.99m,
                UnitInStock = 55,
                Discount = 8,
                ViewCount = 38,
                CommentCount = 0,
                CreatedAt = DateTime.Parse("2021-03-28T00:00:00Z", CultureInfo.InvariantCulture),
                GameGenres = new List<GameGenre>
                {
                    GetGameGenreByName("Strategy"),
                    GetGameGenreByName("TBS"),
                },
                GamePlatforms = new List<GamePlatform>
                {
                    GetGamePlatformByType("Desktop"),
                },
                PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190005"),
            },
        ];
    }

    public static List<Game> GetGames(ICollection<Guid> ids)
    {
        return GetGames().Where(g => ids.Contains(g.Id)).ToList();
    }

    public static Game GetGame()
    {
        return new Game
        {
            Id = Guid.Parse("9ed3a711-0e27-434b-bc87-3a48a93d276c"),
            ProductId = 1,
            Name = "Adventure Quest",
            Key = "adventure-quest",
            Description = "Classic point-and-click adventure game.",
            Price = 27.99m,
            UnitInStock = 45,
            Discount = 0,
            ViewCount = 38,
            CommentCount = 0,
            CreatedAt = DateTime.Parse("2021-03-28T00:00:00Z", CultureInfo.InvariantCulture),
            GameGenres = new List<GameGenre>
            {
                GetGameGenreByName("Adventure"),
            },
            GamePlatforms = new List<GamePlatform>
            {
                GetGamePlatformByType("Browser"),
            },
            PublisherId = Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190004"),
            ImageUrl = "https://example.com/images/adventure-quest.png",
        };
    }

    public static Game GetGame(Guid gameId)
    {
        return GetGames().FirstOrDefault(g => g.Id == gameId);
    }

    public static List<Game> GetGamesByGenre(Guid genreId)
    {
        return GetGames()
            .Where(g => g.GameGenres.Any(gg => gg.GenreId == genreId))
            .ToList();
    }

    public static List<Game> GetGamesByCategory(int? categoryId)
    {
        return GetGames()
            .Where(g => categoryId.HasValue && g.CategoryId == categoryId)
            .ToList();
    }

    public static List<Game> GetGamesByPlatform(Guid platformId)
    {
        return GetGames()
            .Where(g => g.GamePlatforms.Any(gp => gp.PlatformId == platformId))
            .ToList();
    }

    public static List<Game> GetGamesByPublisher(Guid publisherId)
    {
        return GetGames()
            .Where(g => g.PublisherId == publisherId)
            .ToList();
    }

    public static List<Genre> GetGameGenres(Game game)
    {
        var genreIds = game.GameGenres.Select(gg => gg.GenreId).ToList();
        return GenreTestData.GetGenres(genreIds);
    }

    public static List<Platform> GetGamePlatforms(Game game)
    {
        var platformIds = game.GamePlatforms.Select(gp => gp.PlatformId).ToList();
        return PlatformTestData.GetPlatforms(platformIds);
    }

    public static CreateGameRequest GetCreateRequest(string? key = null)
    {
        return new CreateGameRequest
        {
            Game = new GameCreateDto
            {
                Name = "New Game",
                Key = key ?? "new-game-key",
                Description = "Description for the new game.",
                Price = 59.99m,
                UnitInStock = 20,
                Discount = 20,
            },
            Genres = new List<EntityId>
            {
                new(GetGenreByName("Action").Id),
                new(GetGenreByName("RPG").Id),
            },
            Platforms = new List<Guid>
            {
                GetPlatformByType("Desktop").Id,
            },
            Publisher = new EntityId(Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190005")),
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAADICAYAAABS39xVAAAACXBIWXMAAAsTAAALEwEAmpwYAAAClklEQVR4nO3BMQEAAADCoPVPbQwfoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIC3AcUIAAHrs9k8AAAAAElFTkSuQmCC",
        };
    }

    public static CreateGameRequest GetInvalidCreateRequest()
    {
        return new CreateGameRequest
        {
            Game = new GameCreateDto
            {
                Name = string.Empty,
                Key = "invalid-game-key",
                Description = "Invalid game description.",
                Price = 0m,
                UnitInStock = 0,
                Discount = 0,
            },
            Genres = new List<EntityId> { new() },
            Platforms = new List<Guid> { Guid.NewGuid() },
            Publisher = new EntityId(),
        };
    }

    public static UpdateGameRequest GetUpdateRequest(string? id = null)
    {
        return new UpdateGameRequest
        {
            Game = new GameUpdateDto
            {
                Id = !string.IsNullOrEmpty(id) ? EntityId.Parse(id) : new EntityId(Guid.NewGuid()),
                Name = "Updated Game Name",
                Key = "updated-game-key",
                Description = "Updated description for the game.",
                Price = 49.99m,
                UnitInStock = 15,
                Discount = 15,
            },
            Genres = new List<EntityId>
            {
                new(GetGenreByName("Sports").Id),
            },
            Platforms = new List<Guid>
            {
                GetPlatformByType("Console").Id,
                GetPlatformByType("Desktop").Id,
            },
            Publisher = new EntityId(Guid.Parse("5f1f13d7-0f9d-4c5e-9879-1a9b7d190003")),
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAADICAYAAABS39xVAAAACXBIWXMAAAsTAAALEwEAmpwYAAAClklEQVR4nO3BMQEAAADCoPVPbQwfoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIC3AcUIAAHrs9k8AAAAAElFTkSuQmCC",
        };
    }

    public static UpdateGameRequest GetInvalidUpdateRequest(Guid? id = null)
    {
        return new UpdateGameRequest
        {
            Game = new GameUpdateDto
            {
                Id = new EntityId(id ?? Guid.NewGuid()),
                Name = string.Empty,
                Key = "updated-game-key",
                Description = "Updated description for the game.",
                Price = 0m,
                UnitInStock = 0,
                Discount = 0,
            },
            Genres = new List<EntityId> { new(), new() },
            Platforms = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            Publisher = new EntityId(),
        };
    }

    private static Platform GetPlatformByType(string type)
    {
        return PlatformTestData.GetPlatformByType(type);
    }

    private static Genre GetGenreByName(string name)
    {
        return GenreTestData.GetGenreByName(name);
    }

    private static GamePlatform GetGamePlatformByType(string type)
    {
        var platform = PlatformTestData.GetPlatformByType(type);
        return new GamePlatform { PlatformId = platform.Id };
    }

    private static GameGenre GetGameGenreByName(string name)
    {
        var genre = GenreTestData.GetGenreByName(name);
        return new GameGenre { GenreId = genre.Id };
    }
}