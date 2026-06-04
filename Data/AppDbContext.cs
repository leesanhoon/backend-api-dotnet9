using backend_api_dotnet9.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<PrintType> PrintTypes => Set<PrintType>();
    public DbSet<ProductPrintOption> ProductPrintOptions => Set<ProductPrintOption>();
    public DbSet<DesignFile> DesignFiles => Set<DesignFile>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.StockQuantity).HasDefaultValue(0);
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

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("materials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<ProductMaterial>(entity =>
        {
            entity.ToTable("product_materials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExtraPrice).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.ProductId, x.MaterialId }).IsUnique();
            entity.HasOne(x => x.Product).WithMany(x => x.ProductMaterials).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Material).WithMany(x => x.ProductMaterials).HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PrintType>(entity =>
        {
            entity.ToTable("print_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<ProductPrintOption>(entity =>
        {
            entity.ToTable("product_print_options");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExtraPrice).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.ProductId, x.PrintTypeId }).IsUnique();
            entity.HasOne(x => x.Product).WithMany(x => x.ProductPrintOptions).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PrintType).WithMany(x => x.ProductPrintOptions).HasForeignKey(x => x.PrintTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DesignFile>(entity =>
        {
            entity.ToTable("design_files");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FileType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UploadedAtUtc).HasColumnName("uploaded_at_utc");
            entity.HasOne(x => x.Product).WithMany(x => x.DesignFiles).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(50);
            entity.Property(x => x.CustomerEmail).HasMaxLength(255);
            entity.Property(x => x.Note).HasMaxLength(2000);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("draft");
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany(x => x.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Material).WithMany(x => x.OrderItems).HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PrintType).WithMany(x => x.OrderItems).HasForeignKey(x => x.PrintTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DesignFile).WithMany(x => x.OrderItems).HasForeignKey(x => x.DesignFileId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}


