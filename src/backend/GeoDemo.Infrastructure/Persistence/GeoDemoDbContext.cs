using GeoDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GeoDemo.Infrastructure.Persistence;

/// <summary>
/// Stores map features in PostgreSQL with PostGIS support.
/// </summary>
public sealed class GeoDemoDbContext(DbContextOptions<GeoDemoDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the persisted map features.
    /// </summary>
    public DbSet<MapFeature> MapFeatures => Set<MapFeature>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeoDemoDbContext).Assembly);
    }
}