using AssetMon.Common;
using AssetMon.Infrastructure.Entities;

namespace AssetMon.Infrastructure.Persistence;

public class AssetMonDbContext : DbContext
{
    public AssetMonDbContext(DbContextOptions<AssetMonDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>()
            .HasKey(c => new { c.UnitId, c.TripId });
    }

    public DbSet<Trip> Trips { get; set; }

    public DbSet<UnitEvent> UnitEvents { get; set; }
}
