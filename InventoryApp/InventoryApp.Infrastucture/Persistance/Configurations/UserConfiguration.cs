using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryApp.Infrastucture.Persistance.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(User));

        builder.HasKey(u => u.Id);

        builder.Property(u => u.IsBlocked)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(ConfigurationConstants.DefaultStringLength)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(ConfigurationConstants.MaxStringLength)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql(ConfigurationConstants.GetUtcDateSql)
            .IsRequired();
    }
}
