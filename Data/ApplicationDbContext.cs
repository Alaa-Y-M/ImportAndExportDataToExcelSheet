using Microsoft.EntityFrameworkCore;

namespace Task.UI.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<CiscoPSSProducts> CiscoPSSProducts { get; private set; } = null!;
    public DbSet<CiscoPSSServices> CiscoPSSServices { get; private set; } = null!;
    public DbSet<Citrix3PPSS> Citrix3PPSS { get; private set; } = null!;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CiscoPSSProducts>().HasIndex(c => c.PartSKU).IsUnique(true);
        modelBuilder.Entity<CiscoPSSServices>().HasIndex(c => c.PartSKU).IsUnique(true);
        modelBuilder.Entity<Citrix3PPSS>().HasIndex(c => c.PartSKU).IsUnique(true);
    }
}