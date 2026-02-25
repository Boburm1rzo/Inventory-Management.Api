using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class InventoryAccessConfiguration : IEntityTypeConfiguration<InventoryAccess>
{
    public void Configure(EntityTypeBuilder<InventoryAccess> builder)
    {
        builder.ToTable(nameof(InventoryAccess));

        builder.HasKey(x => x.Id);

        builder.HasOne(ia => ia.Inventory)
            .WithMany(i => i.AccessList)
            .HasForeignKey(ia => ia.InventoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(ia => ia.User)
            .WithMany(u => u.AccessList)
            .HasForeignKey(ia => ia.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
