using Gamestore.Domain.Entities.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public const int MaxNameLength = 50;

    private const string TableName = "Genres";

    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Name)
            .HasMaxLength(MaxNameLength)
            .IsRequired();

        builder.Property(g => g.ParentGenreId)
            .IsRequired(false);

        builder.HasIndex(g => g.Name).IsUnique();

        builder.HasOne(g => g.ParentGenre)
            .WithMany(g => g.SubGenres)
            .HasForeignKey(g => g.ParentGenreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
