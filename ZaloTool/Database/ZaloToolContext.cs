using Microsoft.EntityFrameworkCore;
using ZaloTool.Models;

namespace ZaloTool.Database;

public class ZaloToolContext : DbContext
{
    public DbSet<AccountZalo> AccountZalos { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=ZaloToolDb.db");
        optionsBuilder.UseLazyLoadingProxies();
    }
}