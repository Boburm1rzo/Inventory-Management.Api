using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class FieldConfiguration : IEntityTypeConfiguration<InventoryField>
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

        builder
            .HasMany(f => f.Values)
            .WithOne(v => v.Field)
            .HasForeignKey(v => v.FieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
