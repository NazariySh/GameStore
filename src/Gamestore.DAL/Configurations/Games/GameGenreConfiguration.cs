using Gamestore.Domain.Entities.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class GameGenreConfiguration : IEntityTypeConfiguration<GameGenre>
{
    private const string TableName = "GameGenres";

    public void Configure(EntityTypeBuilder<GameGenre> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(gg => new { gg.GameId, gg.GenreId });

        builder.HasOne(gg => gg.Game)
               .WithMany(g => g.GameGenres)
               .HasForeignKey(gg => gg.GameId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gg => gg.Genre)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
    }
}