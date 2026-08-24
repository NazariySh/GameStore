using Gamestore.Domain.Entities.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public const int MaxNameLength = 50;
    public const int MaxKeyLength = 25;
    public const int MaxDescriptionLength = 500;

    private const string TableName = "Games";

    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Name)
            .HasMaxLength(MaxNameLength)
            .IsRequired();

        builder.Property(g => g.Key)
            .HasMaxLength(MaxKeyLength)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasMaxLength(MaxDescriptionLength)
            .IsRequired(false);

        builder.Property(g => g.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(g => g.UnitInStock)
            .IsRequired();

        builder.Property(g => g.Discount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(g => g.ViewCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(g => g.CommentCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(g => g.PublisherId)
            .IsRequired();

        builder.Property(g => g.ImageUrl)
            .IsRequired(false);

        builder.Property(g => g.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(g => g.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(g => g.Key).IsUnique();

        builder.HasIndex(g => g.Price);
        builder.HasIndex(g => g.CreatedAt);

        builder.HasOne(g => g.Publisher)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PublisherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
