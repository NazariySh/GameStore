using Gamestore.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Orders;

public sealed class OrderGameConfiguration : IEntityTypeConfiguration<OrderGame>
{
    private const string TableName = "OrderGames";

    public void Configure(EntityTypeBuilder<OrderGame> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(og => new { og.OrderId, og.ProductId });

        builder.HasAlternateKey(og => og.Id);

        builder.Property(og => og.Id)
            .ValueGeneratedOnAdd();

        builder.Property(og => og.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(og => og.Quantity)
            .IsRequired();

        builder.Property(og => og.Discount)
            .IsRequired(false);

        builder.HasOne(og => og.Order)
            .WithMany(o => o.OrderGames)
            .HasForeignKey(og => og.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}