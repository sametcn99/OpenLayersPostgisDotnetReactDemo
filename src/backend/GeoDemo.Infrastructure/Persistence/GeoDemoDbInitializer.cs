using GeoDemo.Domain.Entities;
using GeoDemo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.IO;

namespace GeoDemo.Infrastructure.Persistence;

/// <summary>
/// Applies database initialization steps and seeds a few demo geometries.
/// </summary>
public sealed class GeoDemoDbInitializer(GeoDemoDbContext dbContext)
{
    private static readonly WKTReader WktReader = new(NtsGeometryServices.Instance);

    /// <summary>
    /// Ensures the schema exists and the initial dataset is present.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

        if (appliedMigrations.Any() || pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (await dbContext.MapFeatures.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.MapFeatures.AddRange(CreateSeedFeatures());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<MapFeature> CreateSeedFeatures()
    {
        return
        [
            CreateSeedFeature("Galata Tower", "Historic point seed for the default map view.", "POINT(28.9744 41.0256)"),
            CreateSeedFeature("Kadikoy Ferry", "Second point seed near the Bosphorus.", "POINT(29.0226 40.9919)"),
            CreateSeedFeature("Waterfront Route", "Sample path across the historic peninsula.", "LINESTRING(28.9654 41.0094, 28.9725 41.0130, 28.9806 41.0182)"),
            CreateSeedFeature("Golden Horn Route", "Second line sample for editing and styling.", "LINESTRING(28.9481 41.0212, 28.9552 41.0280, 28.9684 41.0314)"),
            CreateSeedFeature("Historic Peninsula", "Polygon seed covering the initial viewport.", "POLYGON((28.9586 41.0031, 28.9870 41.0031, 28.9870 41.0231, 28.9586 41.0231, 28.9586 41.0031))"),
            CreateSeedFeature("Moda Coast", "Second polygon seed on the Anatolian side.", "POLYGON((29.0161 40.9810, 29.0410 40.9810, 29.0410 40.9958, 29.0161 40.9958, 29.0161 40.9810))"),
        ];
    }

    private static MapFeature CreateSeedFeature(string name, string description, string wkt)
    {
        var geometry = WktReader.Read(wkt);
        return MapFeature.Create(name, description, geometry, FeatureSource.Seed);
    }
}