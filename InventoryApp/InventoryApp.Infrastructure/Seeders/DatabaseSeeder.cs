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
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (await context.Inventories.AnyAsync()) return;

        // ─── 1. Roles ─────────────────────────────────────────────────────
        await SeedRolesAsync(roleManager);

        // ─── 2. Users ─────────────────────────────────────────────────────
        var (admin, alice, bob, carol, david, eve) = await SeedUsersAsync(userManager);

        // ─── 3. Categories ────────────────────────────────────────────────
        var categories = await SeedCategoriesAsync(context);

        // ─── 4. Tags ──────────────────────────────────────────────────────
        var tags = await SeedTagsAsync(context);

        // ─── 5. Inventories + Fields + IdFormatParts ──────────────────────
        var inventories = await SeedInventoriesAsync(context, alice, bob, carol, david, categories, tags);

        // ─── 6. Items ─────────────────────────────────────────────────────
        var items = await SeedItemsAsync(context, inventories, alice, bob, carol, david);

        // ─── 7. InventoryAccess ───────────────────────────────────────────
        await SeedAccessAsync(context, inventories, alice, bob, carol, david, eve);

        // ─── 8. Likes ─────────────────────────────────────────────────────
        await SeedLikesAsync(context, items, alice, bob, carol, david, eve);

        // ─── 9. Posts ─────────────────────────────────────────────────────
        await SeedPostsAsync(context, inventories, alice, bob, carol, david);
    }

    // ─── Roles ────────────────────────────────────────────────────────────
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "User" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // ─── Users ────────────────────────────────────────────────────────────
    private static async Task<(User, User, User, User, User, User)> SeedUsersAsync(
        UserManager<User> userManager)
    {
        var usersData = new[]
        {
            new { Email = "admin@inventoryapp.com", DisplayName = "Admin User",    Password = "Admin@123", Role = "Admin", IsBlocked = false, DaysAgo = 90 },
            new { Email = "alice@example.com",       DisplayName = "Alice Johnson", Password = "User@123",  Role = "User",  IsBlocked = false, DaysAgo = 60 },
            new { Email = "bob@example.com",         DisplayName = "Bob Smith",     Password = "User@123",  Role = "User",  IsBlocked = false, DaysAgo = 45 },
            new { Email = "carol@example.com",       DisplayName = "Carol White",   Password = "User@123",  Role = "User",  IsBlocked = false, DaysAgo = 30 },
            new { Email = "david@example.com",       DisplayName = "David Brown",   Password = "User@123",  Role = "User",  IsBlocked = false, DaysAgo = 20 },
            new { Email = "eve@example.com",         DisplayName = "Eve Davis",     Password = "User@123",  Role = "User",  IsBlocked = true,  DaysAgo = 10 },
        };

        var createdUsers = new List<User>();

        foreach (var u in usersData)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing is not null) { createdUsers.Add(existing); continue; }

            var user = new User
            {
                UserName = u.Email,
                Email = u.Email,
                DisplayName = u.DisplayName,
                EmailConfirmed = true,
                IsBlocked = u.IsBlocked,
                CreatedAt = DateTime.UtcNow.AddDays(-u.DaysAgo)
            };

            await userManager.CreateAsync(user, u.Password);
            await userManager.AddToRoleAsync(user, u.Role);
            createdUsers.Add(user);
        }

        return (createdUsers[0], createdUsers[1], createdUsers[2],
                createdUsers[3], createdUsers[4], createdUsers[5]);
    }

    // ─── Categories ───────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(AppDbContext context)
    {
        var names = new[] { "Equipment", "Furniture", "Books", "Software", "Vehicles", "Other" };
        var categories = names.ToDictionary(n => n, n => new Category { Name = n });
        await context.Categories.AddRangeAsync(categories.Values);
        await context.SaveChangesAsync();
        return categories;
    }

    // ─── Tags ─────────────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Tag>> SeedTagsAsync(AppDbContext context)
    {
        var names = new[]
        {
            "office", "it", "hardware", "software", "2024", "new",
            "used", "urgent", "maintenance", "library", "hr", "finance", "furniture"
        };
        var tags = names.ToDictionary(n => n, n => new Tag { Name = n });
        await context.Tags.AddRangeAsync(tags.Values);
        await context.SaveChangesAsync();
        return tags;
    }

    // ─── Inventories ──────────────────────────────────────────────────────
    private static async Task<List<Inventory>> SeedInventoriesAsync(
        AppDbContext context,
        User alice, User bob, User carol, User david,
        Dictionary<string, Category> categories,
        Dictionary<string, Tag> tags)
    {
        var inventories = new List<Inventory>
        {
            // ── 1. Office Laptops (Alice) ──────────────────────────────────
            new()
            {
                Title       = "Office Laptops",
                Description = "All company laptops tracked by serial number and assignment status.",
                IsPublic    = true,
                OwnerId     = alice.Id,
                CategoryId  = categories["Equipment"].Id,
                CreatedAt   = DateTime.UtcNow.AddDays(-55),
                UpdatedAt   = DateTime.UtcNow.AddDays(-2),
                InventoryTags =
                [
                    new InventoryTag { Tag = tags["office"] },
                    new InventoryTag { Tag = tags["it"] },
                    new InventoryTag { Tag = tags["hardware"] }
                ],
                Fields =
                [
                    new InventoryField { Title = "Model",         Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 0 },
                    new InventoryField { Title = "Serial Number", Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 1 },
                    new InventoryField { Title = "Price (USD)",   Type = FieldType.Numeric,        DisplayInTable = true,  Order = 2 },
                    new InventoryField { Title = "In Use",        Type = FieldType.Boolean,        DisplayInTable = true,  Order = 3 },
                    new InventoryField { Title = "Notes",         Type = FieldType.MultiLineText,  DisplayInTable = false, Order = 4 },
                ],
                IdFormatParts =
                [
                    new InventoryIdFormatPart { Type = IdFormatPartType.FixedText, Config = "LAP-", Order = 0 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.Sequence,  Config = "000",  Order = 1 },
                ]
            },

            // ── 2. Library Books (Bob) ─────────────────────────────────────
            new()
            {
                Title       = "Library Books",
                Description = "Company library catalog with availability tracking.",
                IsPublic    = true,
                OwnerId     = bob.Id,
                CategoryId  = categories["Books"].Id,
                CreatedAt   = DateTime.UtcNow.AddDays(-40),
                UpdatedAt   = DateTime.UtcNow.AddDays(-5),
                InventoryTags =
                [
                    new InventoryTag { Tag = tags["library"] },
                    new InventoryTag { Tag = tags["office"] }
                ],
                Fields =
                [
                    new InventoryField { Title = "Book Title",  Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 0 },
                    new InventoryField { Title = "Author",      Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 1 },
                    new InventoryField { Title = "Year",        Type = FieldType.Numeric,        DisplayInTable = true,  Order = 2 },
                    new InventoryField { Title = "Available",   Type = FieldType.Boolean,        DisplayInTable = true,  Order = 3 },
                    new InventoryField { Title = "Description", Type = FieldType.MultiLineText,  DisplayInTable = false, Order = 4 },
                ],
                IdFormatParts =
                [
                    new InventoryIdFormatPart { Type = IdFormatPartType.FixedText, Config = "BK-", Order = 0 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.Sequence,  Config = "000", Order = 1 },
                ]
            },

            // ── 3. Office Furniture (Carol) ────────────────────────────────
            new()
            {
                Title       = "Office Furniture",
                Description = "Furniture inventory by floor and room location.",
                IsPublic    = true,
                OwnerId     = carol.Id,
                CategoryId  = categories["Furniture"].Id,
                CreatedAt   = DateTime.UtcNow.AddDays(-28),
                UpdatedAt   = DateTime.UtcNow.AddDays(-3),
                InventoryTags =
                [
                    new InventoryTag { Tag = tags["furniture"] },
                    new InventoryTag { Tag = tags["office"] }
                ],
                Fields =
                [
                    new InventoryField { Title = "Item Name",      Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 0 },
                    new InventoryField { Title = "Location",       Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 1 },
                    new InventoryField { Title = "Purchase Price", Type = FieldType.Numeric,        DisplayInTable = true,  Order = 2 },
                    new InventoryField { Title = "Condition",      Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 3 },
                    new InventoryField { Title = "Notes",          Type = FieldType.MultiLineText,  DisplayInTable = false, Order = 4 },
                ],
                IdFormatParts =
                [
                    new InventoryIdFormatPart { Type = IdFormatPartType.FixedText, Config = "FRN-", Order = 0 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.Sequence,  Config = "000",  Order = 1 },
                ]
            },

            // ── 4. Software Licenses (David) ───────────────────────────────
            new()
            {
                Title       = "Software Licenses",
                Description = "All software licenses, renewal dates and annual costs.",
                IsPublic    = false,
                OwnerId     = david.Id,
                CategoryId  = categories["Software"].Id,
                CreatedAt   = DateTime.UtcNow.AddDays(-18),
                UpdatedAt   = DateTime.UtcNow.AddDays(-1),
                InventoryTags =
                [
                    new InventoryTag { Tag = tags["software"] },
                    new InventoryTag { Tag = tags["it"] },
                    new InventoryTag { Tag = tags["finance"] }
                ],
                Fields =
                [
                    new InventoryField { Title = "Software Name", Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 0 },
                    new InventoryField { Title = "License Key",   Type = FieldType.SingleLineText, DisplayInTable = false, Order = 1 },
                    new InventoryField { Title = "Cost/Year",     Type = FieldType.Numeric,        DisplayInTable = true,  Order = 2 },
                    new InventoryField { Title = "Active",        Type = FieldType.Boolean,        DisplayInTable = true,  Order = 3 },
                    new InventoryField { Title = "Vendor URL",    Type = FieldType.Link,           DisplayInTable = false, Order = 4 },
                ],
                IdFormatParts =
                [
                    new InventoryIdFormatPart { Type = IdFormatPartType.FixedText, Config = "LIC-", Order = 0 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.Sequence,  Config = "000",  Order = 1 },
                ]
            },

            // ── 5. HR Documents (Alice) ────────────────────────────────────
            new()
            {
                Title       = "HR Documents",
                Description = "HR policies, contracts and employee documents registry.",
                IsPublic    = false,
                OwnerId     = alice.Id,
                CategoryId  = categories["Other"].Id,
                CreatedAt   = DateTime.UtcNow.AddDays(-10),
                UpdatedAt   = DateTime.UtcNow.AddDays(-1),
                InventoryTags =
                [
                    new InventoryTag { Tag = tags["hr"] },
                    new InventoryTag { Tag = tags["2024"] }
                ],
                Fields =
                [
                    new InventoryField { Title = "Document Name", Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 0 },
                    new InventoryField { Title = "Category",      Type = FieldType.SingleLineText, DisplayInTable = true,  Order = 1 },
                    new InventoryField { Title = "Confidential",  Type = FieldType.Boolean,        DisplayInTable = true,  Order = 2 },
                    new InventoryField { Title = "Document URL",  Type = FieldType.Link,           DisplayInTable = false, Order = 3 },
                    new InventoryField { Title = "Notes",         Type = FieldType.MultiLineText,  DisplayInTable = false, Order = 4 },
                ],
                IdFormatParts =
                [
                    new InventoryIdFormatPart { Type = IdFormatPartType.FixedText, Config = "HR-",  Order = 0 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.DateTime,  Config = "yyyy", Order = 1 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.FixedText, Config = "-",    Order = 2 },
                    new InventoryIdFormatPart { Type = IdFormatPartType.Sequence,  Config = "000",  Order = 3 },
                ]
            }
        };

        await context.Inventories.AddRangeAsync(inventories);
        await context.SaveChangesAsync();
        return inventories;
    }

    // ─── Items ────────────────────────────────────────────────────────────
    private static async Task<List<Item>> SeedItemsAsync(
        AppDbContext context,
        List<Inventory> inventories,
        User alice, User bob, User carol, User david)
    {
        var allItems = new List<Item>();

        // ── Laptops ───────────────────────────────────────────────────────
        allItems.AddRange(await AddItemsAsync(context, inventories[0],
        [
            ("LAP-001", alice, new() { ["Model"] = "MacBook Pro 16\"",    ["Serial Number"] = "MBP-2024-001", ["Price (USD)"] = 2499.00m, ["In Use"] = true,  ["Notes"] = "CEO's laptop. Do not reassign." }),
            ("LAP-002", alice, new() { ["Model"] = "Dell XPS 15",         ["Serial Number"] = "DXP-2024-002", ["Price (USD)"] = 1799.00m, ["In Use"] = true,  ["Notes"] = "Engineering team lead" }),
            ("LAP-003", alice, new() { ["Model"] = "ThinkPad X1 Carbon",  ["Serial Number"] = "TPX-2024-003", ["Price (USD)"] = 1650.00m, ["In Use"] = true,  ["Notes"] = "Finance department" }),
            ("LAP-004", alice, new() { ["Model"] = "HP EliteBook 840",    ["Serial Number"] = "HPE-2024-004", ["Price (USD)"] = 1200.00m, ["In Use"] = false, ["Notes"] = "In storage, needs battery replacement" }),
            ("LAP-005", alice, new() { ["Model"] = "Surface Pro 9",       ["Serial Number"] = "SFP-2024-005", ["Price (USD)"] = 1599.00m, ["In Use"] = true,  ["Notes"] = "Design team" }),
            ("LAP-006", alice, new() { ["Model"] = "MacBook Air M2",      ["Serial Number"] = "MBA-2024-006", ["Price (USD)"] = 1299.00m, ["In Use"] = true,  ["Notes"] = "Marketing department" }),
            ("LAP-007", alice, new() { ["Model"] = "ASUS ZenBook Pro",    ["Serial Number"] = "AZP-2024-007", ["Price (USD)"] = 1450.00m, ["In Use"] = false, ["Notes"] = "Awaiting OS reinstall" }),
            ("LAP-008", alice, new() { ["Model"] = "Razer Blade 15",      ["Serial Number"] = "RBL-2024-008", ["Price (USD)"] = 2499.00m, ["In Use"] = false, ["Notes"] = "Development machine - pending setup" }),
            ("LAP-009", alice, new() { ["Model"] = "Acer Swift 3",        ["Serial Number"] = "ASW-2024-009", ["Price (USD)"] = 649.00m,  ["In Use"] = true,  ["Notes"] = "HR department" }),
            ("LAP-010", alice, new() { ["Model"] = "LG Gram 17",          ["Serial Number"] = "LGG-2024-010", ["Price (USD)"] = 1350.00m, ["In Use"] = true,  ["Notes"] = "Logistics team" }),
        ]));

        // ── Books ─────────────────────────────────────────────────────────
        allItems.AddRange(await AddItemsAsync(context, inventories[1],
        [
            ("BK-001", bob, new() { ["Book Title"] = "Clean Code",                ["Author"] = "Robert C. Martin",        ["Year"] = 2008m, ["Available"] = true,  ["Description"] = "A handbook of agile software craftsmanship." }),
            ("BK-002", bob, new() { ["Book Title"] = "The Pragmatic Programmer",  ["Author"] = "David Thomas, Andy Hunt", ["Year"] = 2019m, ["Available"] = false, ["Description"] = "Your journey to mastery." }),
            ("BK-003", bob, new() { ["Book Title"] = "Design Patterns",           ["Author"] = "Gang of Four",            ["Year"] = 1994m, ["Available"] = true,  ["Description"] = "Elements of reusable object-oriented software." }),
            ("BK-004", bob, new() { ["Book Title"] = "Domain-Driven Design",      ["Author"] = "Eric Evans",              ["Year"] = 2003m, ["Available"] = true,  ["Description"] = "Tackling complexity in the heart of software." }),
            ("BK-005", bob, new() { ["Book Title"] = "The Phoenix Project",       ["Author"] = "Gene Kim",                ["Year"] = 2013m, ["Available"] = false, ["Description"] = "A novel about IT, DevOps and business." }),
            ("BK-006", bob, new() { ["Book Title"] = "Refactoring",               ["Author"] = "Martin Fowler",           ["Year"] = 2018m, ["Available"] = true,  ["Description"] = "Improving the design of existing code." }),
            ("BK-007", bob, new() { ["Book Title"] = "C# in Depth",               ["Author"] = "Jon Skeet",               ["Year"] = 2019m, ["Available"] = true,  ["Description"] = "Deep dive into C# language features." }),
            ("BK-008", bob, new() { ["Book Title"] = "SQL Performance Explained", ["Author"] = "Markus Winand",           ["Year"] = 2012m, ["Available"] = true,  ["Description"] = "Everything about SQL indexes." }),
            ("BK-009", bob, new() { ["Book Title"] = "Microservices Patterns",    ["Author"] = "Chris Richardson",        ["Year"] = 2018m, ["Available"] = false, ["Description"] = "With examples in Java." }),
            ("BK-010", bob, new() { ["Book Title"] = "You Don't Know JS",         ["Author"] = "Kyle Simpson",            ["Year"] = 2015m, ["Available"] = true,  ["Description"] = "JavaScript deep dive series." }),
        ]));

        // ── Furniture ─────────────────────────────────────────────────────
        allItems.AddRange(await AddItemsAsync(context, inventories[2],
        [
            ("FRN-001", carol, new() { ["Item Name"] = "Standing Desk",    ["Location"] = "Floor 2, Room 201",       ["Purchase Price"] = 650.00m,  ["Condition"] = "Excellent", ["Notes"] = "Height adjustable, motorized" }),
            ("FRN-002", carol, new() { ["Item Name"] = "Ergonomic Chair",  ["Location"] = "Floor 1, Room 105",       ["Purchase Price"] = 420.00m,  ["Condition"] = "Good",      ["Notes"] = "Lumbar support, armrests" }),
            ("FRN-003", carol, new() { ["Item Name"] = "Conference Table", ["Location"] = "Floor 3, Meeting Room A", ["Purchase Price"] = 1200.00m, ["Condition"] = "Good",      ["Notes"] = "Seats 10 people" }),
            ("FRN-004", carol, new() { ["Item Name"] = "Filing Cabinet",   ["Location"] = "Floor 1, HR Office",      ["Purchase Price"] = 180.00m,  ["Condition"] = "Fair",      ["Notes"] = "2 drawers, lockable" }),
            ("FRN-005", carol, new() { ["Item Name"] = "Bookshelf",        ["Location"] = "Floor 2, Library",        ["Purchase Price"] = 250.00m,  ["Condition"] = "Excellent", ["Notes"] = "5 shelves, oak finish" }),
            ("FRN-006", carol, new() { ["Item Name"] = "Whiteboard",       ["Location"] = "Floor 3, Room 302",       ["Purchase Price"] = 90.00m,   ["Condition"] = "Good",      ["Notes"] = "180x90cm magnetic" }),
            ("FRN-007", carol, new() { ["Item Name"] = "Lounge Sofa",      ["Location"] = "Floor 1, Lobby",          ["Purchase Price"] = 800.00m,  ["Condition"] = "Excellent", ["Notes"] = "3-seater, navy blue" }),
            ("FRN-008", carol, new() { ["Item Name"] = "Monitor Stand",    ["Location"] = "Floor 2, Room 210",       ["Purchase Price"] = 75.00m,   ["Condition"] = "Good",      ["Notes"] = "Dual monitor support" }),
            ("FRN-009", carol, new() { ["Item Name"] = "Reception Desk",   ["Location"] = "Floor 1, Entrance",       ["Purchase Price"] = 950.00m,  ["Condition"] = "Excellent", ["Notes"] = "Custom built, L-shaped" }),
            ("FRN-010", carol, new() { ["Item Name"] = "Storage Cabinet",  ["Location"] = "Floor 2, Kitchen",        ["Purchase Price"] = 320.00m,  ["Condition"] = "Fair",      ["Notes"] = "Minor scratches on top" }),
        ]));

        // ── Software Licenses ─────────────────────────────────────────────
        allItems.AddRange(await AddItemsAsync(context, inventories[3],
        [
            ("LIC-001", david, new() { ["Software Name"] = "Microsoft 365",        ["License Key"] = "M365-XXXX-001", ["Cost/Year"] = 1200.00m, ["Active"] = true,  ["Vendor URL"] = "https://microsoft.com" }),
            ("LIC-002", david, new() { ["Software Name"] = "JetBrains Suite",      ["License Key"] = "JB-2024-001",   ["Cost/Year"] = 680.00m,  ["Active"] = true,  ["Vendor URL"] = "https://jetbrains.com" }),
            ("LIC-003", david, new() { ["Software Name"] = "Adobe Creative Cloud", ["License Key"] = "ACC-ENT-001",   ["Cost/Year"] = 2400.00m, ["Active"] = true,  ["Vendor URL"] = "https://adobe.com" }),
            ("LIC-004", david, new() { ["Software Name"] = "Slack Business",       ["License Key"] = "SLK-BIZ-001",   ["Cost/Year"] = 960.00m,  ["Active"] = true,  ["Vendor URL"] = "https://slack.com" }),
            ("LIC-005", david, new() { ["Software Name"] = "GitHub Enterprise",    ["License Key"] = "GHE-ENT-001",   ["Cost/Year"] = 1500.00m, ["Active"] = true,  ["Vendor URL"] = "https://github.com" }),
            ("LIC-006", david, new() { ["Software Name"] = "Figma Organization",   ["License Key"] = "FIG-ORG-001",   ["Cost/Year"] = 720.00m,  ["Active"] = true,  ["Vendor URL"] = "https://figma.com" }),
            ("LIC-007", david, new() { ["Software Name"] = "Zoom Business",        ["License Key"] = "ZM-BIZ-001",    ["Cost/Year"] = 480.00m,  ["Active"] = false, ["Vendor URL"] = "https://zoom.us" }),
            ("LIC-008", david, new() { ["Software Name"] = "Postman Team",         ["License Key"] = "PM-TEAM-001",   ["Cost/Year"] = 360.00m,  ["Active"] = true,  ["Vendor URL"] = "https://postman.com" }),
            ("LIC-009", david, new() { ["Software Name"] = "1Password Teams",      ["License Key"] = "1PW-TEAM-001",  ["Cost/Year"] = 240.00m,  ["Active"] = true,  ["Vendor URL"] = "https://1password.com" }),
            ("LIC-010", david, new() { ["Software Name"] = "Jira Software",        ["License Key"] = "JIRA-ENT-001",  ["Cost/Year"] = 840.00m,  ["Active"] = true,  ["Vendor URL"] = "https://atlassian.com" }),
        ]));

        // ── HR Documents ──────────────────────────────────────────────────
        allItems.AddRange(await AddItemsAsync(context, inventories[4],
        [
            ("HR-2024-001", alice, new() { ["Document Name"] = "Employee Handbook 2024",      ["Category"] = "Policy",   ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/handbook",    ["Notes"] = "Updated January 2024" }),
            ("HR-2024-002", alice, new() { ["Document Name"] = "NDA Template",                ["Category"] = "Legal",    ["Confidential"] = true,  ["Document URL"] = "https://drive.google.com/hr/nda",         ["Notes"] = "Review with legal before use" }),
            ("HR-2024-003", alice, new() { ["Document Name"] = "Remote Work Policy",          ["Category"] = "Policy",   ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/remote",      ["Notes"] = "Effective March 2024" }),
            ("HR-2024-004", alice, new() { ["Document Name"] = "Benefits Guide",              ["Category"] = "Benefits", ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/benefits",    ["Notes"] = "Health, dental, vision included" }),
            ("HR-2024-005", alice, new() { ["Document Name"] = "Code of Conduct",             ["Category"] = "Policy",   ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/conduct",     ["Notes"] = "All employees must sign" }),
            ("HR-2024-006", alice, new() { ["Document Name"] = "Salary Bands 2024",           ["Category"] = "Finance",  ["Confidential"] = true,  ["Document URL"] = "https://drive.google.com/hr/salary",      ["Notes"] = "Restricted to HR and management" }),
            ("HR-2024-007", alice, new() { ["Document Name"] = "Hiring Process Guide",        ["Category"] = "Hiring",   ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/hiring",      ["Notes"] = "For hiring managers only" }),
            ("HR-2024-008", alice, new() { ["Document Name"] = "Performance Review Template", ["Category"] = "Review",   ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/review",      ["Notes"] = "Semi-annual reviews" }),
            ("HR-2024-009", alice, new() { ["Document Name"] = "Onboarding Checklist",        ["Category"] = "Hiring",   ["Confidential"] = false, ["Document URL"] = "https://drive.google.com/hr/onboarding",  ["Notes"] = "First 30 days plan" }),
            ("HR-2024-010", alice, new() { ["Document Name"] = "Termination Procedure",       ["Category"] = "Legal",    ["Confidential"] = true,  ["Document URL"] = "https://drive.google.com/hr/termination", ["Notes"] = "Confidential - HR only" }),
        ]));

        return allItems;
    }

    // ─── Access ───────────────────────────────────────────────────────────
    private static async Task SeedAccessAsync(
        AppDbContext context,
        List<Inventory> inventories,
        User alice, User bob, User carol, User david, User eve)
    {
        // Laptops (Alice owns) → Bob va Carol ga ruxsat
        context.InventoryAccesses.AddRange(
            new InventoryAccess { InventoryId = inventories[0].Id, UserId = bob.Id, GrantedAt = DateTime.UtcNow.AddDays(-20) },
            new InventoryAccess { InventoryId = inventories[0].Id, UserId = carol.Id, GrantedAt = DateTime.UtcNow.AddDays(-15) }
        );

        // Books (Bob owns) → Alice va David ga ruxsat
        context.InventoryAccesses.AddRange(
            new InventoryAccess { InventoryId = inventories[1].Id, UserId = alice.Id, GrantedAt = DateTime.UtcNow.AddDays(-30) },
            new InventoryAccess { InventoryId = inventories[1].Id, UserId = david.Id, GrantedAt = DateTime.UtcNow.AddDays(-10) }
        );

        // Software (David owns, private) → Alice ga ruxsat
        context.InventoryAccesses.Add(
            new InventoryAccess { InventoryId = inventories[3].Id, UserId = alice.Id, GrantedAt = DateTime.UtcNow.AddDays(-5) }
        );

        // HR (Alice owns, private) → Carol ga ruxsat
        context.InventoryAccesses.Add(
            new InventoryAccess { InventoryId = inventories[4].Id, UserId = carol.Id, GrantedAt = DateTime.UtcNow.AddDays(-8) }
        );

        await context.SaveChangesAsync();
    }

    // ─── Likes ────────────────────────────────────────────────────────────
    private static async Task SeedLikesAsync(
        AppDbContext context,
        List<Item> items,
        User alice, User bob, User carol, User david, User eve)
    {
        var users = new[] { alice, bob, carol, david, eve };
        var likes = new List<Like>();
        var rnd = Random.Shared;

        foreach (var item in items)
        {
            var likerCount = rnd.Next(0, users.Length + 1);
            foreach (var user in users.OrderBy(_ => rnd.Next()).Take(likerCount))
            {
                if (!likes.Any(l => l.ItemId == item.Id && l.UserId == user.Id))
                    likes.Add(new Like { ItemId = item.Id, UserId = user.Id });
            }
        }

        await context.Likes.AddRangeAsync(likes);
        await context.SaveChangesAsync();
    }

    // ─── Posts ────────────────────────────────────────────────────────────
    private static async Task SeedPostsAsync(
        AppDbContext context,
        List<Inventory> inventories,
        User alice, User bob, User carol, User david)
    {
        var posts = new List<Post>
        {
            // ── Laptops discussion ─────────────────────────────────────────
            new() { InventoryId = inventories[0].Id, AuthorId = alice.Id,  Content = "All laptops have been audited. Please update the **In Use** field if you've received a new device.",          CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { InventoryId = inventories[0].Id, AuthorId = bob.Id,    Content = "LAP-004 needs a battery replacement ASAP. I've submitted a ticket to IT.",                                    CreatedAt = DateTime.UtcNow.AddDays(-8)  },
            new() { InventoryId = inventories[0].Id, AuthorId = carol.Id,  Content = "Can someone update the location for LAP-007? It was moved to storage last week.",                             CreatedAt = DateTime.UtcNow.AddDays(-5)  },
            new() { InventoryId = inventories[0].Id, AuthorId = alice.Id,  Content = "Updated! Also added notes for LAP-008 — it's pending OS setup before deployment.",                           CreatedAt = DateTime.UtcNow.AddDays(-4)  },
            new() { InventoryId = inventories[0].Id, AuthorId = david.Id,  Content = "Reminder: all laptops need **antivirus** installed before being marked as In Use ✅",                        CreatedAt = DateTime.UtcNow.AddDays(-2)  },

            // ── Books discussion ───────────────────────────────────────────
            new() { InventoryId = inventories[1].Id, AuthorId = bob.Id,    Content = "Just added 3 new books! Check out *Clean Architecture* by Robert Martin — highly recommended.",              CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { InventoryId = inventories[1].Id, AuthorId = alice.Id,  Content = "I'd like to borrow **Domain-Driven Design**. Is BK-004 available?",                                          CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { InventoryId = inventories[1].Id, AuthorId = bob.Id,    Content = "Yes! BK-004 is available. Mark it as unavailable once you pick it up please.",                               CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { InventoryId = inventories[1].Id, AuthorId = carol.Id,  Content = "Suggestion: add a **Due Date** field to track when books need to be returned 📚",                            CreatedAt = DateTime.UtcNow.AddDays(-7)  },
            new() { InventoryId = inventories[1].Id, AuthorId = david.Id,  Content = "Great idea Carol! Also recommend adding *The Staff Engineer's Path* — excellent read for leads.",            CreatedAt = DateTime.UtcNow.AddDays(-3)  },

            // ── Furniture discussion ───────────────────────────────────────
            new() { InventoryId = inventories[2].Id, AuthorId = carol.Id,  Content = "Annual furniture audit completed. All items verified except FRN-010 which has minor damage.",                CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { InventoryId = inventories[2].Id, AuthorId = alice.Id,  Content = "FRN-010 damage noted. Should we get a repair quote or just replace it?",                                    CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { InventoryId = inventories[2].Id, AuthorId = carol.Id,  Content = "Getting a repair quote first. Will update the notes field once we hear back from the vendor.",               CreatedAt = DateTime.UtcNow.AddDays(-17) },
            new() { InventoryId = inventories[2].Id, AuthorId = bob.Id,    Content = "The new standing desks on Floor 2 are amazing! Great investment 💪",                                        CreatedAt = DateTime.UtcNow.AddDays(-5)  },

            // ── Software discussion ────────────────────────────────────────
            new() { InventoryId = inventories[3].Id, AuthorId = david.Id,  Content = "**Zoom Business** license (LIC-007) deactivated. We're switching to Google Meet company-wide.",             CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { InventoryId = inventories[3].Id, AuthorId = alice.Id,  Content = "Noted. Should we also evaluate the Postman Team license? Most devs use the free tier anyway.",               CreatedAt = DateTime.UtcNow.AddDays(-13) },
            new() { InventoryId = inventories[3].Id, AuthorId = david.Id,  Content = "Good point. I'll check usage stats and report back before the next renewal in Q3.",                         CreatedAt = DateTime.UtcNow.AddDays(-13) },
            new() { InventoryId = inventories[3].Id, AuthorId = carol.Id,  Content = "Adobe CC renewal coming up next month. Budget has been approved ✅",                                         CreatedAt = DateTime.UtcNow.AddDays(-2)  },
        };

        await context.Posts.AddRangeAsync(posts);
        await context.SaveChangesAsync();
    }

    // ─── Helper ───────────────────────────────────────────────────────────
    private static async Task<List<Item>> AddItemsAsync(
        AppDbContext context,
        Inventory inventory,
        IEnumerable<(string CustomId, User Creator, Dictionary<string, object> Values)> itemsData)
    {
        var fields = inventory.Fields.ToList();
        var createdItems = new List<Item>();

        foreach (var (customId, creator, values) in itemsData)
        {
            var item = new Item
            {
                CustomId = customId,
                InventoryId = inventory.Id,
                CreatedById = creator.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
                UpdatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 5)),
            };

            foreach (var field in fields)
            {
                if (!values.TryGetValue(field.Title, out var rawValue)) continue;

                var fv = new ItemFieldValue { FieldId = field.Id };

                switch (field.Type)
                {
                    case FieldType.SingleLineText:
                    case FieldType.MultiLineText:
                    case FieldType.Link:
                        fv.TextValue = rawValue.ToString();
                        break;
                    case FieldType.Numeric:
                        fv.NumericValue = Convert.ToDecimal(rawValue);
                        break;
                    case FieldType.Boolean:
                        fv.BooleanValue = (bool)rawValue;
                        break;
                }

                item.FieldValues.Add(fv);
            }

            await context.Items.AddAsync(item);
            createdItems.Add(item);
        }

        await context.SaveChangesAsync();
        return createdItems;
    }
}
