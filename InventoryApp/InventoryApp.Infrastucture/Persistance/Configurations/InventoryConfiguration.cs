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

        builder.HasOne(i => i.Owner)
            .WithMany(u => u.OwnedInventories)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Inventories)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(i => i.Title)
            .HasMaxLength(ConfigurationConstants.DefaultStringLength)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(ConfigurationConstants.MaxStringLength)
            .IsRequired(false);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(ConfigurationConstants.MaxStringLength)
            .IsRequired(false);

        builder.Property(i => i.IsPublic)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();

        builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
