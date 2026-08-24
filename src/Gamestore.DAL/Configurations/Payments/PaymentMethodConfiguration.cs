using Gamestore.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Payments;

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    private const int MaxTitleLength = 50;
    private const int MaxDescriptionLength = 500;
    private const int MaxImageUrlLength = 500;

    private const string TableName = "PaymentMethods";

    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pm => pm.Title)
            .HasMaxLength(MaxTitleLength)
            .IsRequired();

        builder.Property(pm => pm.Description)
            .HasMaxLength(MaxDescriptionLength)
            .IsRequired();

        builder.Property(pm => pm.ImageUrl)
            .HasMaxLength(MaxImageUrlLength)
            .IsRequired();

        builder.HasIndex(pm => pm.Title).IsUnique();
    }
}