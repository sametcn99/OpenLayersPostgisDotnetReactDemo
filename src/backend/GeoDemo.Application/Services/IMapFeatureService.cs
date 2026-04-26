using GeoDemo.Application.DTOs;

namespace GeoDemo.Application.Services;

/// <summary>
/// Provides use cases for browsing and editing map features.
/// </summary>
public interface IMapFeatureService
{
    /// <summary>
    /// Returns all map features as GeoJSON-ready DTOs.
    /// </summary>
    Task<IReadOnlyList<MapFeatureDto>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Returns a single map feature by its identifier.
    /// </summary>
    Task<MapFeatureDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user-owned map feature.
    /// </summary>
    Task<MapFeatureDto> CreateAsync(SaveMapFeatureInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing map feature when it exists.
    /// </summary>
    Task<MapFeatureDto?> UpdateAsync(Guid id, SaveMapFeatureInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an existing map feature when it exists.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}