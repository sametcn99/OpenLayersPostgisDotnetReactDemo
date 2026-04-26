using GeoDemo.Application.Abstractions;
using GeoDemo.Domain.Entities;
using GeoDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GeoDemo.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for map features.
/// </summary>
public sealed class MapFeatureRepository(GeoDemoDbContext dbContext) : IMapFeatureRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MapFeature>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext.MapFeatures
            .AsNoTracking()
            .OrderBy(feature => feature.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<MapFeature?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.MapFeatures.FirstOrDefaultAsync(feature => feature.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> AnyAsync(CancellationToken cancellationToken)
    {
        return dbContext.MapFeatures.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Add(MapFeature feature)
    {
        dbContext.MapFeatures.Add(feature);
    }

    /// <inheritdoc />
    public void Remove(MapFeature feature)
    {
        dbContext.MapFeatures.Remove(feature);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}