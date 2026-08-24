using System.Linq.Expressions;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Services.Games;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Validators.Games;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Games;

public class GameKeyGeneratorTests
{
    private readonly Mock<IRepository<Game>> _mockGameRepository;
    private readonly GameKeyGenerator _gameKeyGenerator;

    public GameKeyGeneratorTests()
    {
        _mockGameRepository = new Mock<IRepository<Game>>();
        var mockNameNormalizer = new Mock<INameNormalizer>();
        mockNameNormalizer
            .Setup(x => x.Normalize(It.IsAny<string>()))
            .Returns<string>(name => name);

        _gameKeyGenerator = new GameKeyGenerator(
            _mockGameRepository.Object,
            mockNameNormalizer.Object,
            Mock.Of<ILogger<GameKeyGenerator>>());
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task GenerateUniqueAsync_ShouldThrowArgumentException_WhenGameNameIsInvalid(string invalidGameName)
    {
        var act = () => _gameKeyGenerator.GenerateUniqueAsync(invalidGameName);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Theory]
    [MemberData(nameof(UniqueKeyTestData))]
    public async Task GenerateUniqueAsync_ShouldReturnCorrectKey_WhenKeyIsUnique(string gameName, string expectedKey)
    {
        SetupMockRepositoryIsUniqueKey();

        var key = await _gameKeyGenerator.GenerateUniqueAsync(gameName);

        Assert.Equal(expectedKey, key);
    }

    [Theory]
    [MemberData(nameof(NotUniqueKeyTestData))]
    public async Task GenerateUniqueAsync_ShouldAddSuffix_WhenKeyIsNotUnique(string gameName, string expectedKey, int failedAttempts)
    {
        SetupMockRepositoryIsUniqueKeySequence(failedAttempts);

        var key = await _gameKeyGenerator.GenerateUniqueAsync(gameName);

        Assert.Equal(expectedKey, key);
    }

    [Fact]
    public async Task GenerateUniqueAsync_ShouldTruncateKey_WhenKeyIsTooLong()
    {
        var longName = new string('a', 100) + " " + new string('b', 100);
        var expectedKey = new string('a', GameValidationRules.MaxKeyLength);

        SetupMockRepositoryIsUniqueKey();

        var key = await _gameKeyGenerator.GenerateUniqueAsync(longName);

        Assert.Equal(expectedKey, key);
        Assert.Equal(GameValidationRules.MaxKeyLength, key.Length);
    }

    [Fact]
    public async Task GenerateUniqueAsync_ShouldThrowGameKeyGenerationException_WhenMaxRetriesExceeded()
    {
        const string gameName = "Super Mario";

        SetupMockRepositoryNotUniqueKey();

        var act = () => _gameKeyGenerator.GenerateUniqueAsync(gameName);

        await Assert.ThrowsAsync<GameKeyGenerationException>(act);
    }

    public static TheoryData<string, string> UniqueKeyTestData()
    {
        return new TheoryData<string, string>
        {
            { "Super Mario", "Super Mario" },
            { "The Legend of Zelda", "The Legend of Zelda" },
            { "Final Fantasy VII", "Final Fantasy VII" },
            { "Super Mario & Friends! 2023", "Super Mario & Friends! 20" },
            { "Game with Special Characters #1", "Game with Special Charact" },
        };
    }

    public static TheoryData<string, string, int> NotUniqueKeyTestData()
    {
        return new TheoryData<string, string, int>
        {
            { "Super Mario", "Super Mario_4", 4 },
            { "The Legend of Zelda", "The Legend of Zelda_2", 2 },
            { "Final Fantasy VII", "Final Fantasy VII_5", 5 },
            { "Super Mario & Friends! 2023", "Super Mario & Friends! _4", 4 },
            { "Game with Special Characters #1", "Game with Special Chara_3", 3 },
        };
    }

    private void SetupMockRepositoryNotUniqueKey()
    {
        SetupMockRepositoryIsUniqueKey(false);
    }

    private void SetupMockRepositoryIsUniqueKey(bool result = true)
    {
        _mockGameRepository
            .Setup(x => x.NotExistsAsync(
                It.IsAny<Expression<Func<Game, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private void SetupMockRepositoryIsUniqueKeySequence(int failedAttempts)
    {
        var sequence = _mockGameRepository
            .SetupSequence(x => x.NotExistsAsync(
                It.IsAny<Expression<Func<Game, bool>>>(),
                It.IsAny<CancellationToken>()));

        for (var i = 0; i < failedAttempts; i++)
        {
            sequence = sequence.ReturnsAsync(false);
        }

        sequence.ReturnsAsync(true);
    }
}