using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable(nameof(Like));

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Item)
            .WithMany(i => i.Likes)
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();
    }
}
