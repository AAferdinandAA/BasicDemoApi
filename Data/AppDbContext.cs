using Microsoft.EntityFrameworkCore;
using BasicDemoApi.Models;

namespace BasicDemoApi.Data;

public class AppDbContext : DbContext
{
    // 构造函数
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 声明DbSet（实体集合）
    public DbSet<User> Users => Set<User>();

    // 重写OnModelCreating方法，添加时间字段配置
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.CreateTime)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.ToUniversalTime(), // 写入数据库时转为UTC
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // 从数据库读取时指定为UTC
            );

        modelBuilder.Entity<User>()
            .Property(u => u.CreateTime)
            .HasColumnType("timestamp with time zone")
            .HasConversion(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

        // 其他配置
    }
}