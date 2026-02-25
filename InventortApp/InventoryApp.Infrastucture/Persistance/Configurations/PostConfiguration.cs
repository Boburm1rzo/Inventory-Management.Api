using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable(nameof(Post));

        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.Inventory)
            .WithMany(i => i.Posts)
            .HasForeignKey(p => p.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(p => p.Author)
            .WithMany(a => a.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Property(p => p.Content)
            .HasMaxLength(ConfigurationConstants.MaxStringLength)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();
    }
}
