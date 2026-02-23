using InventoryApp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastucture.Persistance;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<User> Users { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemFieldValue> ItemFieldValues { get; set; }
    public DbSet<InventoryAccess> InventoryAccesses { get; set; }
    public DbSet<InventoryField> InventoryFields { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<InventoryIdFormatPart> InventoryIdFormatParts { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
