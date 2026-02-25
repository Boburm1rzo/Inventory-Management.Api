using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastructure.Persistance.Configurations;

internal sealed class InventoryTagConfiguration : IEntityTypeConfiguration<InventoryTag>
{
    public void Configure(EntityTypeBuilder<InventoryTag> builder)
    {
        builder.ToTable(nameof(InventoryTag));

        builder.HasKey(x => new { x.InventoryId, x.TagId });

        builder.HasOne(x => x.Inventory)
            .WithMany(x => x.InventoryTags)
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.InventoryTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
