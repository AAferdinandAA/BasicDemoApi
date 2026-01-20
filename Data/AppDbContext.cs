using Microsoft.EntityFrameworkCore;
using BasicsDemoApi.Models;

namespace BasicsDemoApi.Data;

public class AppDbContext : DbContext
{
    // 接收配置（用于读取连接字符串）
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 对应数据库的Users表
    public DbSet<User> Users => Set<User>();
}