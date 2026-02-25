using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastructure.Persistance.Configurations;

internal sealed class ItemFieldValueConfiguration : IEntityTypeConfiguration<ItemFieldValue>
{
    public void Configure(EntityTypeBuilder<ItemFieldValue> builder)
    {
        builder.ToTable(nameof(ItemFieldValue));

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Item)
            .WithMany(i => i.FieldValues)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.TextValue)
            .HasMaxLength(ConfigurationConstants.MaxStringLength);

        builder.Property(x => x.NumericValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.BooleanValue)
            .HasDefaultValue(false);
    }
}
