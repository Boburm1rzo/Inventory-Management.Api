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

        builder
            .HasMany(u => u.OwnedInventories)
            .WithOne(i => i.Owner)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.AccessList)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.Likes)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.Posts)
            .WithOne(p => p.Author)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.Items)
            .WithOne(i => i.CreatedBy)
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
