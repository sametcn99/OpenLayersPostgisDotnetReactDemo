using GeoDemo.Domain.Enums;
using NetTopologySuite.Geometries;

namespace GeoDemo.Domain.Entities;

/// <summary>
/// Represents a user-visible feature that can be drawn and persisted on the map.
/// </summary>
public sealed class MapFeature
{
    private const int SupportedSrid = 4326;

    private MapFeature()
    {
    }

    private MapFeature(string name, string? description, Geometry geometry, FeatureSource source)
    {
        Id = Guid.NewGuid();
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        Geometry = NormalizeGeometry(geometry);
        Source = source;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    /// <summary>
    /// Gets the unique feature identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the user-facing feature name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional feature description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the persisted geometry in EPSG:4326.
    /// </summary>
    public Geometry Geometry { get; private set; } = default!;

    /// <summary>
    /// Gets the origin of the feature.
    /// </summary>
    public FeatureSource Source { get; private set; }

    /// <summary>
    /// Gets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the last update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Creates a new feature instance after validating the provided geometry.
    /// </summary>
    public static MapFeature Create(string name, string? description, Geometry geometry, FeatureSource source)
    {
        return new MapFeature(name, description, geometry, source);
    }

    /// <summary>
    /// Replaces the feature contents with a new name, description, and geometry.
    /// </summary>
    public void Update(string name, string? description, Geometry geometry)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        Geometry = NormalizeGeometry(geometry);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static Geometry NormalizeGeometry(Geometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        if (geometry.IsEmpty)
        {
            throw new ArgumentException("Geometry cannot be empty.", nameof(geometry));
        }

        if (geometry is not Point && geometry is not LineString && geometry is not Polygon)
        {
            throw new ArgumentException("Only Point, LineString, and Polygon geometries are supported.", nameof(geometry));
        }

        var normalizedGeometry = (Geometry)geometry.Copy();
        normalizedGeometry.SRID = SupportedSrid;
        return normalizedGeometry;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Feature name is required.", nameof(name));
        }

        return name.Trim();
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}