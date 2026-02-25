using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class InventoryFieldConfiguration : IEntityTypeConfiguration<InventoryField>
{
    public void Configure(EntityTypeBuilder<InventoryField> builder)
    {
        builder.ToTable(nameof(InventoryField));

        builder.HasKey(f => f.Id);

        builder
            .HasOne(f => f.Inventory)
            .WithMany(i => i.Fields)
            .HasForeignKey(f => f.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(f => new { f.InventoryId, f.Order });
        builder.HasIndex(f => new { f.InventoryId, f.Title });

        builder.Property(f => f.Title)
            .HasMaxLength(ConfigurationConstants.DefaultStringLength)
            .IsRequired();

        builder.Property(f => f.Description)
            .HasMaxLength(ConfigurationConstants.MaxStringLength)
            .IsRequired(false);

        builder.Property(f => f.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.DisplayInTable)
            .IsRequired();

        builder.Property(f => f.Order)
            .IsRequired();
    }
}
