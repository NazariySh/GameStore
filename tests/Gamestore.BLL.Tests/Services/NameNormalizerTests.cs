using Gamestore.BLL.Services;

namespace Gamestore.BLL.Tests.Services;

public class NameNormalizerTests
{
    private readonly NameNormalizer _nameNormalizer;

    public NameNormalizerTests()
    {
        _nameNormalizer = new NameNormalizer();
    }

    [Theory]
    [MemberData(nameof(NameTestData))]
    public void Normalize_ShouldReturnCorrectNormalizedString(string name, string expectedName)
    {
        var result = _nameNormalizer.Normalize(name);

        Assert.Equal(expectedName, result);
    }

    public static TheoryData<string, string> NameTestData()
    {
        return new TheoryData<string, string>
        {
            { "Super Mario", "super-mario" },
            { "The Legend of Zelda", "the-legend-of-zelda" },
            { "Final Fantasy VII", "final-fantasy-vii" },
            { "Super Mario & Friends! 2023", "super-mario-friends-2023" },
            { "Game with Special Characters #1", "game-with-special-characters-1" },
            { "Pokémon Red", "pokémon-red" },
            { "   Trim   Spaces   ", "trim-spaces" },
            { "MULTI case NAME", "multi-case-name" },
            { "Танчики 1990", "танчики-1990" },
            { "ギャラガ 88", "ギャラガ-88" },
            { "Crash Bandicoot N. Sane Trilogy", "crash-bandicoot-n-sane-trilogy" },
            { "Diablo II: Resurrected", "diablo-ii-resurrected" },
            { "Resident Evil 4 Remake", "resident-evil-4-remake" },
            { "Age of Empires IV", "age-of-empires-iv" },
            { "God-of-War", "god-of-war" },
            { "Halo: Combat Evolved", "halo-combat-evolved" },
            { "Half-Life 2", "half-life-2" },
            { "Super Smash Bros. Ultimate", "super-smash-bros-ultimate" },
        };
    }
}