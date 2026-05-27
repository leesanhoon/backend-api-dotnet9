using backend_api_dotnet9.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("todo_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsDone).HasDefaultValue(false);
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        });
    }
}
