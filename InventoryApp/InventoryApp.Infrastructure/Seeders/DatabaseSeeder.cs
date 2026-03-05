using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Enums;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApp.Infrastructure.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Migration avtomatik qo'llanadi
        await context.Database.MigrateAsync();

        // Har bir resurs alohida tekshiriladi
        await SeedRolesAsync(roleManager);
        var users = await SeedUsersAsync(userManager);
        var categories = await SeedCategoriesAsync(context);  // ← endi duplicate bo'lmaydi
        var tags = await SeedTagsAsync(context);              // ← endi duplicate bo'lmaydi

        // Faqat inventarlar uchun early return
        if (await context.Inventories.AnyAsync()) return;

        var inventories = await SeedInventoriesAsync(context, users, categories, tags);
        await SeedItemsAsync(context, inventories, users);
        await context.SaveChangesAsync();

    }

    // ─── Roles ────────────────────────────────────────────────────────────
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ─── Users ────────────────────────────────────────────────────────────
    private static async Task<List<User>> SeedUsersAsync(UserManager<User> userManager)
    {
        var usersData = new[]
        {
            new { Email = "admin@inventoryapp.com",   DisplayName = "Admin User",     Password = "Admin@123",  IsAdmin = true  },
            new { Email = "alice@example.com",        DisplayName = "Alice Johnson",   Password = "User@123",   IsAdmin = false },
            new { Email = "bob@example.com",          DisplayName = "Bob Smith",       Password = "User@123",   IsAdmin = false },
            new { Email = "carol@example.com",        DisplayName = "Carol White",     Password = "User@123",   IsAdmin = false },
            new { Email = "david@example.com",        DisplayName = "David Brown",     Password = "User@123",   IsAdmin = false },
        };

        var users = new List<User>();

        foreach (var u in usersData)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing is not null)
            {
                users.Add(existing);
                continue;
            }

            var user = new User
            {
                UserName = u.Email,
                Email = u.Email,
                DisplayName = u.DisplayName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(10, 60))
            };

            await userManager.CreateAsync(user, u.Password);
            await userManager.AddToRoleAsync(user, u.IsAdmin ? "Admin" : "User");
            users.Add(user);
        }

        return users;
    }

    // ─── Categories ───────────────────────────────────────────────────────
    private static async Task<List<Category>> SeedCategoriesAsync(AppDbContext context)
    {
        var names = new[] { "Equipment", "Furniture", "Books", "Software", "Vehicles", "Other" };

        var existing = await context.Categories.ToListAsync();
        var result = new List<Category>();

        foreach (var name in names)
        {
            var category = existing.FirstOrDefault(c => c.Name == name);
            if (category is null)
            {
                category = new Category { Name = name };
                await context.Categories.AddAsync(category);
            }
            result.Add(category);
        }

        await context.SaveChangesAsync();
        return result;
    }

    // ─── Tags ─────────────────────────────────────────────────────────────
    private static async Task<List<Tag>> SeedTagsAsync(AppDbContext context)
    {
        var names = new[]
        {
        "office", "it", "hardware", "software", "2024", "new",
        "used", "urgent", "maintenance", "library", "hr", "finance", "furniture"
    };

        var existing = await context.Tags.ToListAsync();
        var result = new List<Tag>();

        foreach (var name in names)
        {
            var tag = existing.FirstOrDefault(t => t.Name == name);
            if (tag is null)
            {
                tag = new Tag { Name = name };
                await context.Tags.AddAsync(tag);
            }
            result.Add(tag);
        }

        await context.SaveChangesAsync();
        return result;
    }

    // ─── Inventories ──────────────────────────────────────────────────────
    private static async Task<List<Inventory>> SeedInventoriesAsync(
        AppDbContext context,
        List<User> users,
        List<Category> categories,
        List<Tag> tags)
    {
        var admin = users[0];
        var alice = users[1];
        var bob = users[2];
        var carol = users[3];

        var equipmentCat = categories.First(c => c.Name == "Equipment");
        var furnitureCat = categories.First(c => c.Name == "Furniture");
        var booksCat = categories.First(c => c.Name == "Books");
        var softwareCat = categories.First(c => c.Name == "Software");

        var inventoryDefs = new[]
        {
            new
            {
                Owner = admin,
                Title = "Office Laptops",
                Description = "## Office Laptops Inventory\nTrack all company laptops, their specs and assignment status.",
                Category = equipmentCat,
                IsPublic = true,
                Tags = new[] { "office", "it", "hardware" },
                Fields = new[]
                {
                    (Title: "Model",        Type: FieldType.SingleLineText, Display: true),
                    (Title: "Serial Number",Type: FieldType.SingleLineText, Display: true),
                    (Title: "Price (USD)",  Type: FieldType.Numeric,        Display: true),
                    (Title: "In Use",       Type: FieldType.Boolean,        Display: true),
                    (Title: "Notes",        Type: FieldType.MultiLineText,  Display: false),
                },
                IdPrefix = "LAP-"
            },
            new
            {
                Owner = alice,
                Title = "Library Books",
                Description = "## Library Collection\nAll books available in the company library.",
                Category = booksCat,
                IsPublic = true,
                Tags = new[] { "library", "office" },
                Fields = new[]
                {
                    (Title: "Book Title",   Type: FieldType.SingleLineText, Display: true),
                    (Title: "Author",       Type: FieldType.SingleLineText, Display: true),
                    (Title: "Year",         Type: FieldType.Numeric,        Display: true),
                    (Title: "Available",    Type: FieldType.Boolean,        Display: true),
                    (Title: "Description",  Type: FieldType.MultiLineText,  Display: false),
                },
                IdPrefix = "BK-"
            },
            new
            {
                Owner = bob,
                Title = "Office Furniture",
                Description = "## Furniture Registry\nDesks, chairs, and other furniture in all offices.",
                Category = furnitureCat,
                IsPublic = false,
                Tags = new[] { "office", "furniture" },
                Fields = new[]
                {
                    (Title: "Item Name",    Type: FieldType.SingleLineText, Display: true),
                    (Title: "Location",     Type: FieldType.SingleLineText, Display: true),
                    (Title: "Purchase Price",Type: FieldType.Numeric,       Display: true),
                    (Title: "Condition",    Type: FieldType.SingleLineText, Display: true),
                    (Title: "Notes",        Type: FieldType.MultiLineText,  Display: false),
                },
                IdPrefix = "FRN-"
            },
            new
            {
                Owner = carol,
                Title = "Software Licenses",
                Description = "## Software License Tracker\nManage all software licenses and expiration dates.",
                Category = softwareCat,
                IsPublic = false,
                Tags = new[] { "software", "it", "finance" },
                Fields = new[]
                {
                    (Title: "Software Name",Type: FieldType.SingleLineText, Display: true),
                    (Title: "License Key",  Type: FieldType.SingleLineText, Display: true),
                    (Title: "Cost/Year",    Type: FieldType.Numeric,        Display: true),
                    (Title: "Active",       Type: FieldType.Boolean,        Display: true),
                    (Title: "Vendor URL",   Type: FieldType.Link,           Display: false),
                },
                IdPrefix = "LIC-"
            },
            new
            {
                Owner = admin,
                Title = "HR Documents",
                Description = "## HR Document Registry\nTrack contracts, policies, and official HR documents.",
                Category = categories.First(c => c.Name == "Other"),
                IsPublic = false,
                Tags = new[] { "hr", "finance" },
                Fields = new[]
                {
                    (Title: "Document Name",Type: FieldType.SingleLineText, Display: true),
                    (Title: "Employee",     Type: FieldType.SingleLineText, Display: true),
                    (Title: "Year",         Type: FieldType.Numeric,        Display: true),
                    (Title: "Signed",       Type: FieldType.Boolean,        Display: true),
                    (Title: "Remarks",      Type: FieldType.MultiLineText,  Display: false),
                },
                IdPrefix = "HR-"
            },
        };

        var inventories = new List<Inventory>();

        foreach (var def in inventoryDefs)
        {
            var inventory = new Inventory
            {
                Title = def.Title,
                Description = def.Description,
                IsPublic = def.IsPublic,
                OwnerId = def.Owner.Id,
                CategoryId = def.Category.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(5, 30)),
                UpdatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 5)),
            };

            // Tags
            foreach (var tagName in def.Tags)
            {
                var tag = tags.FirstOrDefault(t => t.Name == tagName);
                if (tag is null) continue;

                inventory.InventoryTags.Add(new InventoryTag
                {
                    Tag = tag
                });
            }

            // Fields
            for (int i = 0; i < def.Fields.Length; i++)
            {
                var f = def.Fields[i];
                inventory.Fields.Add(new InventoryField
                {
                    Title = f.Title,
                    Description = $"Enter {f.Title.ToLower()} for this item",
                    Type = f.Type,
                    DisplayInTable = f.Display,
                    Order = i
                });
            }

            // ID Format: PREFIX + SEQUENCE
            inventory.IdFormatParts.Add(new InventoryIdFormatPart
            {
                Type = IdFormatPartType.FixedText,
                Config = def.IdPrefix,
                Order = 0
            });
            inventory.IdFormatParts.Add(new InventoryIdFormatPart
            {
                Type = IdFormatPartType.Sequence,
                Config = "000",   // leading zeros format
                Order = 1
            });

            inventories.Add(inventory);
            await context.Inventories.AddAsync(inventory);
        }

        await context.SaveChangesAsync();
        return inventories;
    }

    // ─── Items ────────────────────────────────────────────────────────────
    private static async Task SeedItemsAsync(
        AppDbContext context,
        List<Inventory> inventories,
        List<User> users)
    {
        var alice = users[1];
        var bob = users[2];
        var carol = users[3];
        var david = users[4];

        // Laptops
        var laptopInv = inventories[0];
        var laptopItems = new[]
        {
            new { CustomId = "LAP-001", Values = new Dictionary<string, object> { ["Model"] = "Dell XPS 15", ["Serial Number"] = "DXP-2024-001", ["Price (USD)"] = 1299.99m, ["In Use"] = true,  ["Notes"] = "Assigned to marketing team" } },
            new { CustomId = "LAP-002", Values = new Dictionary<string, object> { ["Model"] = "MacBook Pro 14", ["Serial Number"] = "MBP-2024-002", ["Price (USD)"] = 1999.00m, ["In Use"] = true,  ["Notes"] = "Designer workstation" } },
            new { CustomId = "LAP-003", Values = new Dictionary<string, object> { ["Model"] = "ThinkPad X1 Carbon", ["Serial Number"] = "TPX-2024-003", ["Price (USD)"] = 1450.00m, ["In Use"] = false, ["Notes"] = "Available in storage" } },
            new { CustomId = "LAP-004", Values = new Dictionary<string, object> { ["Model"] = "HP EliteBook 840", ["Serial Number"] = "HEB-2024-004", ["Price (USD)"] = 1100.00m, ["In Use"] = true,  ["Notes"] = "Finance department" } },
            new { CustomId = "LAP-005", Values = new Dictionary<string, object> { ["Model"] = "Lenovo IdeaPad 5", ["Serial Number"] = "LIP-2024-005", ["Price (USD)"] = 750.00m,  ["In Use"] = false, ["Notes"] = "Needs repair - screen damage" } },
            new { CustomId = "LAP-006", Values = new Dictionary<string, object> { ["Model"] = "ASUS ZenBook 14", ["Serial Number"] = "AZB-2024-006", ["Price (USD)"] = 899.00m,  ["In Use"] = true,  ["Notes"] = "Remote worker" } },
            new { CustomId = "LAP-007", Values = new Dictionary<string, object> { ["Model"] = "Surface Laptop 5", ["Serial Number"] = "SFL-2024-007", ["Price (USD)"] = 1299.00m, ["In Use"] = true,  ["Notes"] = "Executive laptop" } },
            new { CustomId = "LAP-008", Values = new Dictionary<string, object> { ["Model"] = "Razer Blade 15", ["Serial Number"] = "RBL-2024-008", ["Price (USD)"] = 2499.00m, ["In Use"] = false, ["Notes"] = "Development machine - pending setup" } },
            new { CustomId = "LAP-009", Values = new Dictionary<string, object> { ["Model"] = "Acer Swift 3", ["Serial Number"] = "ASW-2024-009", ["Price (USD)"] = 649.00m,  ["In Use"] = true,  ["Notes"] = "HR department" } },
            new { CustomId = "LAP-010", Values = new Dictionary<string, object> { ["Model"] = "LG Gram 17", ["Serial Number"] = "LGG-2024-010", ["Price (USD)"] = 1350.00m, ["In Use"] = true,  ["Notes"] = "Logistics team" } },
        };

        await AddItemsToInventory(context, laptopInv, laptopItems.Select(i =>
            (i.CustomId, i.Values, Creator: alice)).ToArray());

        // Books
        var bookInv = inventories[1];
        var bookItems = new[]
        {
            new { CustomId = "BK-001", Values = new Dictionary<string, object> { ["Book Title"] = "Clean Code", ["Author"] = "Robert C. Martin", ["Year"] = 2008m, ["Available"] = true, ["Description"] = "A handbook of agile software craftsmanship" } },
            new { CustomId = "BK-002", Values = new Dictionary<string, object> { ["Book Title"] = "The Pragmatic Programmer", ["Author"] = "David Thomas, Andrew Hunt", ["Year"] = 2019m, ["Available"] = false, ["Description"] = "Your journey to mastery" } },
            new { CustomId = "BK-003", Values = new Dictionary<string, object> { ["Book Title"] = "Design Patterns", ["Author"] = "Gang of Four", ["Year"] = 1994m, ["Available"] = true, ["Description"] = "Elements of reusable OO software" } },
            new { CustomId = "BK-004", Values = new Dictionary<string, object> { ["Book Title"] = "Domain-Driven Design", ["Author"] = "Eric Evans", ["Year"] = 2003m, ["Available"] = true, ["Description"] = "Tackling complexity in the heart of software" } },
            new { CustomId = "BK-005", Values = new Dictionary<string, object> { ["Book Title"] = "The Phoenix Project", ["Author"] = "Gene Kim", ["Year"] = 2013m, ["Available"] = false, ["Description"] = "A novel about IT, DevOps, and helping your business win" } },
            new { CustomId = "BK-006", Values = new Dictionary<string, object> { ["Book Title"] = "Refactoring", ["Author"] = "Martin Fowler", ["Year"] = 2018m, ["Available"] = true, ["Description"] = "Improving the design of existing code" } },
            new { CustomId = "BK-007", Values = new Dictionary<string, object> { ["Book Title"] = "C# in Depth", ["Author"] = "Jon Skeet", ["Year"] = 2019m, ["Available"] = true, ["Description"] = "Deep dive into C# language features" } },
            new { CustomId = "BK-008", Values = new Dictionary<string, object> { ["Book Title"] = "SQL Performance Explained", ["Author"] = "Markus Winand", ["Year"] = 2012m, ["Available"] = true, ["Description"] = "Everything about SQL indexes" } },
            new { CustomId = "BK-009", Values = new Dictionary<string, object> { ["Book Title"] = "Microservices Patterns", ["Author"] = "Chris Richardson", ["Year"] = 2018m, ["Available"] = false, ["Description"] = "With examples in Java" } },
            new { CustomId = "BK-010", Values = new Dictionary<string, object> { ["Book Title"] = "You Don't Know JS", ["Author"] = "Kyle Simpson", ["Year"] = 2015m, ["Available"] = true, ["Description"] = "JavaScript deep series" } },
        };

        await AddItemsToInventory(context, bookInv, bookItems.Select(i =>
            (i.CustomId, i.Values, Creator: bob)).ToArray());

        // Furniture
        var furnitureInv = inventories[2];
        var furnitureItems = new[]
        {
            new { CustomId = "FRN-001", Values = new Dictionary<string, object> { ["Item Name"] = "Standing Desk",    ["Location"] = "Floor 2, Room 201", ["Purchase Price"] = 650.00m, ["Condition"] = "Excellent", ["Notes"] = "Height adjustable" } },
            new { CustomId = "FRN-002", Values = new Dictionary<string, object> { ["Item Name"] = "Ergonomic Chair",  ["Location"] = "Floor 1, Room 105", ["Purchase Price"] = 420.00m, ["Condition"] = "Good",      ["Notes"] = "Lumbar support" } },
            new { CustomId = "FRN-003", Values = new Dictionary<string, object> { ["Item Name"] = "Conference Table", ["Location"] = "Floor 3, Meeting Room A", ["Purchase Price"] = 1200.00m, ["Condition"] = "Good", ["Notes"] = "Seats 10 people" } },
            new { CustomId = "FRN-004", Values = new Dictionary<string, object> { ["Item Name"] = "Filing Cabinet",   ["Location"] = "Floor 1, HR Office", ["Purchase Price"] = 180.00m, ["Condition"] = "Fair",      ["Notes"] = "2 drawers, locked" } },
            new { CustomId = "FRN-005", Values = new Dictionary<string, object> { ["Item Name"] = "Bookshelf",        ["Location"] = "Floor 2, Library",   ["Purchase Price"] = 250.00m, ["Condition"] = "Excellent", ["Notes"] = "5 shelves" } },
            new { CustomId = "FRN-006", Values = new Dictionary<string, object> { ["Item Name"] = "Whiteboard",       ["Location"] = "Floor 3, Room 302",  ["Purchase Price"] = 90.00m,  ["Condition"] = "Good",      ["Notes"] = "180x90cm" } },
            new { CustomId = "FRN-007", Values = new Dictionary<string, object> { ["Item Name"] = "Lounge Sofa",      ["Location"] = "Floor 1, Lobby",     ["Purchase Price"] = 800.00m, ["Condition"] = "Excellent", ["Notes"] = "3-seater" } },
            new { CustomId = "FRN-008", Values = new Dictionary<string, object> { ["Item Name"] = "Monitor Stand",    ["Location"] = "Floor 2, Room 210",  ["Purchase Price"] = 75.00m,  ["Condition"] = "Good",      ["Notes"] = "Dual monitor" } },
            new { CustomId = "FRN-009", Values = new Dictionary<string, object> { ["Item Name"] = "Reception Desk",   ["Location"] = "Floor 1, Entrance",  ["Purchase Price"] = 950.00m, ["Condition"] = "Excellent", ["Notes"] = "Custom built" } },
            new { CustomId = "FRN-010", Values = new Dictionary<string, object> { ["Item Name"] = "Storage Cabinet",  ["Location"] = "Floor 2, Kitchen",   ["Purchase Price"] = 320.00m, ["Condition"] = "Fair",      ["Notes"] = "Some scratches" } },
        };

        await AddItemsToInventory(context, furnitureInv, furnitureItems.Select(i =>
            (i.CustomId, i.Values, Creator: carol)).ToArray());

        // Software licenses
        var softwareInv = inventories[3];
        var softwareItems = new[]
        {
            new { CustomId = "LIC-001", Values = new Dictionary<string, object> { ["Software Name"] = "Microsoft 365", ["License Key"] = "XXXXX-XXXXX-XXXXX-365-01", ["Cost/Year"] = 1200.00m, ["Active"] = true,  ["Vendor URL"] = "https://microsoft.com" } },
            new { CustomId = "LIC-002", Values = new Dictionary<string, object> { ["Software Name"] = "JetBrains Suite", ["License Key"] = "JB-2024-SUITE-001", ["Cost/Year"] = 680.00m, ["Active"] = true,  ["Vendor URL"] = "https://jetbrains.com" } },
            new { CustomId = "LIC-003", Values = new Dictionary<string, object> { ["Software Name"] = "Adobe Creative Cloud", ["License Key"] = "ACC-2024-ENT-001", ["Cost/Year"] = 2400.00m, ["Active"] = true,  ["Vendor URL"] = "https://adobe.com" } },
            new { CustomId = "LIC-004", Values = new Dictionary<string, object> { ["Software Name"] = "Slack Business", ["License Key"] = "SLK-BIZ-2024-001", ["Cost/Year"] = 960.00m, ["Active"] = true,  ["Vendor URL"] = "https://slack.com" } },
            new { CustomId = "LIC-005", Values = new Dictionary<string, object> { ["Software Name"] = "GitHub Enterprise", ["License Key"] = "GHE-2024-ENT-001", ["Cost/Year"] = 1500.00m, ["Active"] = true,  ["Vendor URL"] = "https://github.com" } },
            new { CustomId = "LIC-006", Values = new Dictionary<string, object> { ["Software Name"] = "Figma Organization", ["License Key"] = "FIG-ORG-2024-001", ["Cost/Year"] = 720.00m, ["Active"] = true,  ["Vendor URL"] = "https://figma.com" } },
            new { CustomId = "LIC-007", Values = new Dictionary<string, object> { ["Software Name"] = "Zoom Business", ["License Key"] = "ZM-BIZ-2024-001", ["Cost/Year"] = 480.00m, ["Active"] = false, ["Vendor URL"] = "https://zoom.us" } },
            new { CustomId = "LIC-008", Values = new Dictionary<string, object> { ["Software Name"] = "Postman Team", ["License Key"] = "PM-TEAM-2024-001", ["Cost/Year"] = 360.00m, ["Active"] = true,  ["Vendor URL"] = "https://postman.com" } },
            new { CustomId = "LIC-009", Values = new Dictionary<string, object> { ["Software Name"] = "1Password Teams", ["License Key"] = "1PW-TEAM-2024-001", ["Cost/Year"] = 240.00m, ["Active"] = true,  ["Vendor URL"] = "https://1password.com" } },
            new { CustomId = "LIC-010", Values = new Dictionary<string, object> { ["Software Name"] = "Jira Software", ["License Key"] = "JIRA-2024-001", ["Cost/Year"] = 840.00m, ["Active"] = true,  ["Vendor URL"] = "https://atlassian.com" } },
        };

        await AddItemsToInventory(context, softwareInv, softwareItems.Select(i =>
            (i.CustomId, i.Values, Creator: david)).ToArray());

        await context.SaveChangesAsync();
    }

    // ─── Helper: item + field values yaratish ─────────────────────────────
    private static async Task AddItemsToInventory(
        AppDbContext context,
        Inventory inventory,
        IEnumerable<(string CustomId, Dictionary<string, object> Values, User Creator)> items)
    {
        var fields = inventory.Fields.ToList();

        foreach (var (customId, values, creator) in items)
        {
            var item = new Item
            {
                CustomId = customId,
                InventoryId = inventory.Id,
                CreatedById = creator.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 20)),
                UpdatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 5)),
            };

            foreach (var field in fields)
            {
                if (!values.TryGetValue(field.Title, out var rawValue)) continue;

                var fieldValue = new ItemFieldValue
                {
                    FieldId = field.Id,
                };

                switch (field.Type)
                {
                    case FieldType.SingleLineText:
                    case FieldType.MultiLineText:
                    case FieldType.Link:
                        fieldValue.TextValue = rawValue.ToString();
                        break;
                    case FieldType.Numeric:
                        fieldValue.NumericValue = Convert.ToDecimal(rawValue);
                        break;
                    case FieldType.Boolean:
                        fieldValue.BooleanValue = (bool)rawValue;
                        break;
                }

                item.FieldValues.Add(fieldValue);
            }

            await context.Items.AddAsync(item);
        }

        await context.SaveChangesAsync();
    }
}