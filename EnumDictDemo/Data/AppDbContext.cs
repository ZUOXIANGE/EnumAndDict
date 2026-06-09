using EnumDictDemo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnumDictDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SysDict> SysDicts => Set<SysDict>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysDict>(entity =>
        {
            entity.HasIndex(e => new { e.DictCode, e.DictValue }).IsUnique();
            entity.HasIndex(e => e.DictCode);
            entity.HasIndex(e => e.IsEnabled);
        });

        SeedDictData(modelBuilder);
    }

    private static void SeedDictData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysDict>().HasData(
            new SysDict { Id = 1, DictCode = "sex", DictValue = "1", DictLabel = "男", SortOrder = 1, IsEnabled = true },
            new SysDict { Id = 2, DictCode = "sex", DictValue = "2", DictLabel = "女", SortOrder = 2, IsEnabled = true },
            new SysDict { Id = 3, DictCode = "sex", DictValue = "0", DictLabel = "未知", SortOrder = 0, IsEnabled = true },
            new SysDict { Id = 4, DictCode = "nation", DictValue = "1", DictLabel = "汉族", SortOrder = 1, IsEnabled = true },
            new SysDict { Id = 5, DictCode = "nation", DictValue = "2", DictLabel = "蒙古族", SortOrder = 2, IsEnabled = true },
            new SysDict { Id = 6, DictCode = "nation", DictValue = "3", DictLabel = "回族", SortOrder = 3, IsEnabled = true },
            new SysDict { Id = 7, DictCode = "nation", DictValue = "4", DictLabel = "藏族", SortOrder = 4, IsEnabled = true },
            new SysDict { Id = 8, DictCode = "nation", DictValue = "5", DictLabel = "维吾尔族", SortOrder = 5, IsEnabled = true },
            new SysDict { Id = 9, DictCode = "order_source", DictValue = "pc", DictLabel = "PC端", SortOrder = 1, IsEnabled = true },
            new SysDict { Id = 10, DictCode = "order_source", DictValue = "app", DictLabel = "APP端", SortOrder = 2, IsEnabled = true },
            new SysDict { Id = 11, DictCode = "order_source", DictValue = "mini", DictLabel = "小程序", SortOrder = 3, IsEnabled = true },
            new SysDict { Id = 12, DictCode = "order_source", DictValue = "h5", DictLabel = "H5页面", SortOrder = 4, IsEnabled = true }
        );
    }
}