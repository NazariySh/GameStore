using Gamestore.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Users;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const int MaxUserNameLength = 100;
    public const int MinPasswordLength = 8;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UserName)
            .HasMaxLength(MaxUserNameLength)
            .IsRequired();

        builder.Property(u => u.NormalizedUserName)
            .HasMaxLength(MaxUserNameLength)
            .IsRequired();

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}