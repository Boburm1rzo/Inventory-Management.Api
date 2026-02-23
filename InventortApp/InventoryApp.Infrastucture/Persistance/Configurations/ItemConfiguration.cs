using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable(nameof(Item));

        builder.HasKey(i => i.Id);

        builder
            .HasMany(i => i.FieldValues)
            .WithOne(fv => fv.Item)
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.Likes)
            .WithOne(fv => fv.Item)
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(i => i.CreatedBy)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder
            .HasIndex(i => new { i.InventoryId, i.CustomId })
            .IsUnique();
    }
}
