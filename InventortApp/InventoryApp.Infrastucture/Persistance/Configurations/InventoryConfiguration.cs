using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable(nameof(Inventory));

        builder.HasKey(i => i.Id);

        builder
            .HasOne(i => i.Owner)
            .WithMany(u => u.OwnedInventories)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder
            .HasOne(i => i.Category)
            .WithMany(c => c.Inventories)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(i => i.Fields)
            .WithOne(f => f.Inventory)
            .HasForeignKey(f => f.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.IdFormatParts)
            .WithOne(p => p.Inventory)
            .HasForeignKey(p => p.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.AccessList)
            .WithOne(a => a.Inventory)
            .HasForeignKey(a => a.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.InventoryTags)
            .WithOne(it => it.Inventory)
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.Items)
            .WithOne(it => it.Inventory)
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(i => i.Posts)
            .WithOne(it => it.Inventory)
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
