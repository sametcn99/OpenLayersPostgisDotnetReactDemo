using GeoDemo.Application.Abstractions;
using GeoDemo.Application.DTOs;
using GeoDemo.Domain.Entities;
using GeoDemo.Domain.Enums;

namespace GeoDemo.Application.Services;

/// <summary>
/// Implements the application workflow for persisted GeoJSON features.
/// </summary>
public sealed class MapFeatureService(
    IMapFeatureRepository repository,
    IGeometryGeoJsonConverter geometryConverter) : IMapFeatureService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MapFeatureDto>> ListAsync(CancellationToken cancellationToken)
    {
        var features = await repository.ListAsync(cancellationToken);
        return features.Select(Map).ToArray();
    }

    /// <inheritdoc />
    public async Task<MapFeatureDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var feature = await repository.GetByIdAsync(id, cancellationToken);
        return feature is null ? null : Map(feature);
    }

    /// <inheritdoc />
    public async Task<MapFeatureDto> CreateAsync(SaveMapFeatureInput input, CancellationToken cancellationToken)
    {
        var geometry = geometryConverter.Read(input.GeometryJson);
        var feature = MapFeature.Create(input.Name, input.Description, geometry, FeatureSource.User);

        repository.Add(feature);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(feature);
    }

    /// <inheritdoc />
    public async Task<MapFeatureDto?> UpdateAsync(Guid id, SaveMapFeatureInput input, CancellationToken cancellationToken)
    {
        var feature = await repository.GetByIdAsync(id, cancellationToken);
        if (feature is null)
        {
            return null;
        }

        var geometry = geometryConverter.Read(input.GeometryJson);
        feature.Update(input.Name, input.Description, geometry);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(feature);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var feature = await repository.GetByIdAsync(id, cancellationToken);
        if (feature is null)
        {
            return false;
        }

        repository.Remove(feature);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private MapFeatureDto Map(MapFeature feature)
    {
        return new MapFeatureDto(
            feature.Id,
            feature.Name,
            feature.Description,
            geometryConverter.Write(feature.Geometry),
            feature.Geometry.GeometryType,
            feature.Source.ToString(),
            feature.CreatedAtUtc,
            feature.UpdatedAtUtc);
    }
}