using GeoDemo.Domain.Entities;

namespace GeoDemo.Application.Abstractions;

/// <summary>
/// Defines persistence operations for map features.
/// </summary>
public interface IMapFeatureRepository
{
    /// <summary>
    /// Returns all stored features ordered for stable demo rendering.
    /// </summary>
    Task<IReadOnlyList<MapFeature>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns a feature by its identifier when it exists.
    /// </summary>
    Task<MapFeature?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether any feature already exists.
    /// </summary>
    Task<bool> AnyAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new feature to the current unit of work.
    /// </summary>
    void Add(MapFeature feature);

    /// <summary>
    /// Removes a feature from the current unit of work.
    /// </summary>
    void Remove(MapFeature feature);

    /// <summary>
    /// Persists the current unit of work.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}