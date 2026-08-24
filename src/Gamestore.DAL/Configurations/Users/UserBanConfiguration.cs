using Gamestore.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Users;

public sealed class UserBanConfiguration : IEntityTypeConfiguration<UserBan>
{
    public const int MaxUserNameLength = UserConfiguration.MaxUserNameLength;

    private const string TableName = "UserBans";

    public void Configure(EntityTypeBuilder<UserBan> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(ub => ub.Id);

        builder.Property(ub => ub.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ub => ub.UserName)
            .HasMaxLength(MaxUserNameLength)
            .IsRequired();

        builder.Property(ub => ub.BanUntil)
            .IsRequired();

        builder.HasIndex(ub => ub.UserName).IsUnique();
    }
}