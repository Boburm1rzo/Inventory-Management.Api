using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastructure.Persistance.Configurations;

internal sealed class InventoryIdFormatPartConfiguration : IEntityTypeConfiguration<InventoryIdFormatPart>
{
    public void Configure(EntityTypeBuilder<InventoryIdFormatPart> builder)
    {
        builder.ToTable(nameof(InventoryIdFormatPart));

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Inventory)
            .WithMany(i => i.IdFormatParts)
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Config)
            .HasMaxLength(ConfigurationConstants.MaxStringLength)
            .IsRequired(false);
    }
}
