using Api_Key_Authentication.Model;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    public DbSet<ApiKey> ApiKeys { get; set; }

    public DbSet<Product> Products { get; set; }
}
