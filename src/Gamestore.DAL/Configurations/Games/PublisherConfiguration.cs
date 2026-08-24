using Gamestore.Domain.Entities.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
{
    public const int MaxCompanyNameLength = 50;
    public const int MaxHomePageLength = 250;
    public const int MaxDescriptionLength = 500;

    private const string TableName = "Publishers";

    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CompanyName)
            .HasMaxLength(MaxCompanyNameLength)
            .IsRequired();

        builder.Property(p => p.HomePage)
            .HasMaxLength(MaxHomePageLength)
            .IsRequired(false);

        builder.Property(p => p.Description)
            .HasMaxLength(MaxDescriptionLength)
            .IsRequired(false);

        builder.HasIndex(p => p.CompanyName).IsUnique();
    }
}