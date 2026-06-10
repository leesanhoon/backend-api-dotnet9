using backend_api_dotnet9.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Data;

public static class AppDbSeeder
{
    public static async Task SeedSampleDataAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var categories = await EnsureRootCategoriesAsync(dbContext, cancellationToken);
        var subCategories = await EnsureSubCategoriesAsync(dbContext, categories, cancellationToken);
        var lids = await EnsureLidsAsync(dbContext, categories, cancellationToken);
        await EnsureProductsAsync(dbContext, subCategories, lids, cancellationToken);
    }

    private static async Task<Dictionary<string, Category>> EnsureRootCategoriesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var rootNames = new[] { "Ly giấy", "Ly nhựa" };
        var existing = await dbContext.Categories
            .Where(c => rootNames.Contains(c.Name) && c.IsRoot)
            .ToDictionaryAsync(c => c.Name, cancellationToken);

        if (!existing.ContainsKey("Ly giấy"))
        {
            var cat = new Category { Name = "Ly giấy", Description = "Các loại ly giấy", IsRoot = true };
            dbContext.Categories.Add(cat);
            existing["Ly giấy"] = cat;
        }

        if (!existing.ContainsKey("Ly nhựa"))
        {
            var cat = new Category { Name = "Ly nhựa", Description = "Các loại ly nhựa", IsRoot = true };
            dbContext.Categories.Add(cat);
            existing["Ly nhựa"] = cat;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static async Task<Dictionary<string, Category>> EnsureSubCategoriesAsync(
        AppDbContext dbContext,
        Dictionary<string, Category> roots,
        CancellationToken cancellationToken)
    {
        var seeds = new (string Name, string Description, string RootName)[]
        {
            ("Ly giấy 1 lớp", "Ly giấy 1 lớp tiêu chuẩn", "Ly giấy"),
            ("Ly giấy 2 lớp", "Ly giấy 2 lớp giữ nhiệt tốt", "Ly giấy"),
            ("Ly PET", "Ly nhựa PET trong suốt", "Ly nhựa"),
            ("Ly PP", "Ly nhựa PP chịu nhiệt", "Ly nhựa")
        };

        var subNames = seeds.Select(s => s.Name).ToList();
        var existing = await dbContext.Categories
            .Where(c => subNames.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name, cancellationToken);

        foreach (var seed in seeds)
        {
            if (existing.ContainsKey(seed.Name)) continue;

            var cat = new Category
            {
                Name = seed.Name,
                Description = seed.Description,
                ParentId = roots[seed.RootName].Id
            };
            dbContext.Categories.Add(cat);
            existing[seed.Name] = cat;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static async Task<Dictionary<string, Lid>> EnsureLidsAsync(
        AppDbContext dbContext,
        Dictionary<string, Category> roots,
        CancellationToken cancellationToken)
    {
        var lidNames = new[] { "Nắp vòm trong suốt", "Nắp phẳng PP" };
        var existing = await dbContext.Lids
            .Where(l => lidNames.Contains(l.Name))
            .ToDictionaryAsync(l => l.Name, cancellationToken);

        if (!existing.ContainsKey("Nắp vòm trong suốt"))
        {
            var lid = new Lid
            {
                Name = "Nắp vòm trong suốt",
                Description = "Nắp vòm dùng cho đồ uống có topping",
                CategoryId = roots["Ly nhựa"].Id
            };
            lid.Prices.Add(new LidPrice { DiameterMm = 90, SizeName = "Size S (90mm)", UnitPrice = 350 });
            lid.Prices.Add(new LidPrice { DiameterMm = 95, SizeName = "Size M (95mm)", UnitPrice = 380 });
            lid.Prices.Add(new LidPrice { DiameterMm = 106, SizeName = "Size L (106mm)", UnitPrice = 420 });
            dbContext.Lids.Add(lid);
            existing["Nắp vòm trong suốt"] = lid;
        }

        if (!existing.ContainsKey("Nắp phẳng PP"))
        {
            var lid = new Lid
            {
                Name = "Nắp phẳng PP",
                Description = "Nắp phẳng có lỗ cắm ống hút",
                CategoryId = roots["Ly nhựa"].Id
            };
            lid.Prices.Add(new LidPrice { DiameterMm = 90, SizeName = "Size S (90mm)", UnitPrice = 280 });
            lid.Prices.Add(new LidPrice { DiameterMm = 95, SizeName = "Size M (95mm)", UnitPrice = 300 });
            dbContext.Lids.Add(lid);
            existing["Nắp phẳng PP"] = lid;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static async Task EnsureProductsAsync(
        AppDbContext dbContext,
        Dictionary<string, Category> subCategories,
        Dictionary<string, Lid> lids,
        CancellationToken cancellationToken)
    {
        var productNames = new[] { "Ly PET trong suốt", "Ly giấy trắng 1 lớp" };
        var existingNames = await dbContext.Products
            .Where(p => productNames.Contains(p.Name))
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);

        if (!existingNames.Contains("Ly PET trong suốt"))
        {
            var product = new Product
            {
                Name = "Ly PET trong suốt",
                Description = "Ly nhựa PET trong suốt cho đồ uống lạnh",
                CategoryId = subCategories["Ly PET"].Id
            };

            product.Variants.Add(CreateVariant(250, 90, [(1000, 850), (5000, 780), (10000, 720)]));
            product.Variants.Add(CreateVariant(350, 95, [(1000, 950), (5000, 880), (10000, 820)]));
            product.Variants.Add(CreateVariant(500, 106, [(1000, 1100), (5000, 1020), (10000, 950)]));

            product.ProductLids.Add(new ProductLid { Lid = lids["Nắp vòm trong suốt"] });
            product.ProductLids.Add(new ProductLid { Lid = lids["Nắp phẳng PP"] });

            dbContext.Products.Add(product);
        }

        if (!existingNames.Contains("Ly giấy trắng 1 lớp"))
        {
            var product = new Product
            {
                Name = "Ly giấy trắng 1 lớp",
                Description = "Ly giấy trắng tiêu chuẩn cho cà phê mang đi",
                CategoryId = subCategories["Ly giấy 1 lớp"].Id
            };

            product.Variants.Add(CreateVariant(240, 80, [(1000, 750), (5000, 680), (10000, 620)]));
            product.Variants.Add(CreateVariant(360, 90, [(1000, 900), (5000, 830), (10000, 760)]));

            dbContext.Products.Add(product);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ProductVariant CreateVariant(int capacityMl, int diameterMm, (int qty, decimal price)[] tiers)
    {
        var variant = new ProductVariant { CapacityMl = capacityMl, DiameterMm = diameterMm };
        foreach (var (qty, price) in tiers)
        {
            variant.PriceTiers.Add(new VariantPriceTier { MinQuantity = qty, UnitPrice = price });
        }
        return variant;
    }
}
