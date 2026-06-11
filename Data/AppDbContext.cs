using backend_api_dotnet9.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantPriceTier> VariantPriceTiers => Set<VariantPriceTier>();
    public DbSet<Lid> Lids => Set<Lid>();
    public DbSet<LidPrice> LidPrices => Set<LidPrice>();
    public DbSet<ProductLid> ProductLids => Set<ProductLid>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnerImage> PartnerImages => Set<PartnerImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsRoot).HasDefaultValue(false);
            entity.HasIndex(x => new { x.Name, x.ParentId }).IsUnique();
            entity.HasIndex(x => x.ParentId);
            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.AvatarImageUrl).HasColumnName("avatar_image_url").HasMaxLength(1000);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ImageType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("product_variants");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProductId, x.CapacityMl }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VariantPriceTier>(entity =>
        {
            entity.ToTable("variant_price_tiers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.ProductVariantId, x.MinQuantity }).IsUnique();
            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.PriceTiers)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lid>(entity =>
        {
            entity.ToTable("lids");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.CategoryId);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Lids)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LidPrice>(entity =>
        {
            entity.ToTable("lid_prices");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SizeName).HasMaxLength(100);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.LidId, x.DiameterMm }).IsUnique();
            entity.HasOne(x => x.Lid)
                .WithMany(x => x.Prices)
                .HasForeignKey(x => x.LidId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductLid>(entity =>
        {
            entity.ToTable("product_lids");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProductId, x.LidId }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductLids)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Lid)
                .WithMany(x => x.ProductLids)
                .HasForeignKey(x => x.LidId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CustomerEmail).HasMaxLength(200);
            entity.Property(x => x.Note).HasMaxLength(1000);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("partners");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(500).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.AvatarImageUrl).HasColumnName("avatar_image_url").HasMaxLength(1000);
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<PartnerImage>(entity =>
        {
            entity.ToTable("partner_images");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ImageType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.HasOne(x => x.Partner)
                .WithMany(x => x.PartnerImages)
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
