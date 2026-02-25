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

        builder.HasOne(i => i.CreatedBy)
            .WithMany(i => i.Items)
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(i => i.Inventory)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.InventoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(i => new { i.InventoryId, i.CustomId })
            .IsUnique();

        builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();
    }
}
