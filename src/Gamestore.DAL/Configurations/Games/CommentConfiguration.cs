using Gamestore.DAL.Configurations.Users;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gamestore.DAL.Configurations.Games;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public const int MaxNameLength = UserBanConfiguration.MaxUserNameLength;

    private const string TableName = "Comments";

    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasMaxLength(MaxNameLength)
            .IsRequired();

        builder.Property(c => c.Body)
            .IsRequired();

        builder.Property(c => c.Type)
            .HasDefaultValue(CommentType.Root)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.ParentCommentId)
            .IsRequired(false);

        builder.Property(c => c.GameId)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.ChildComments)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}