using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryApp.Infrastructure.Persistance;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Data Source=desktop-fb3ogeq;Initial Catalog=InventoryApp;Integrated Security=True;Trust Server Certificate=True")
            .Options;

        return new AppDbContext(options);
    }
}