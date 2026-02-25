using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable(nameof(Tag));

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(ConfigurationConstants.DefaultStringLength)
            .IsRequired();
    }
}
