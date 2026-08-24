using Gamestore.Domain.Entities.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
    public const int MaxTypeLength = 50;

    private const string TableName = "Platforms";

    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Type)
            .HasMaxLength(MaxTypeLength)
            .IsRequired();

        builder.HasIndex(p => p.Type).IsUnique();
    }
}
