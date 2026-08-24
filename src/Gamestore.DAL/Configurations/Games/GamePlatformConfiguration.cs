using Gamestore.Domain.Entities.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class GamePlatformConfiguration : IEntityTypeConfiguration<GamePlatform>
{
    private const string TableName = "GamePlatforms";

    public void Configure(EntityTypeBuilder<GamePlatform> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(gp => new { gp.GameId, gp.PlatformId });

        builder.HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlatforms)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gp => gp.Platform)
            .WithMany(p => p.GamePlatforms)
            .HasForeignKey(gp => gp.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}