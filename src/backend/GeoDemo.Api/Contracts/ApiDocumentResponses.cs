using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GeoDemo.Api.Contracts;

/// <summary>
/// Represents the API health check response.
/// </summary>
public sealed class HealthStatusResponse
{
    /// <summary>
    /// Gets or sets the current API health status.
    /// </summary>
    [Description("Current API status value.")]
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Represents a GeoJSON feature collection returned by the API.
/// </summary>
public sealed class GeoJsonFeatureCollectionResponse
{
    /// <summary>
    /// Gets or sets the GeoJSON object type.
    /// </summary>
    [JsonPropertyName("type")]
    [Description("GeoJSON object type. Always `FeatureCollection`.")]
    public string Type { get; init; } = "FeatureCollection";

    /// <summary>
    /// Gets or sets the features included in the collection.
    /// </summary>
    [JsonPropertyName("features")]
    [Description("Features currently stored in the application.")]
    public IReadOnlyList<GeoJsonFeatureResponse> Features { get; init; } = [];
}

/// <summary>
/// Represents a single GeoJSON feature returned by the API.
/// </summary>
public sealed class GeoJsonFeatureResponse
{
    /// <summary>
    /// Gets or sets the GeoJSON object type.
    /// </summary>
    [JsonPropertyName("type")]
    [Description("GeoJSON object type. Always `Feature`.")]
    public string Type { get; init; } = "Feature";

    /// <summary>
    /// Gets or sets the unique feature identifier.
    /// </summary>
    [JsonPropertyName("id")]
    [Description("Unique identifier of the feature.")]
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the GeoJSON geometry object.
    /// </summary>
    [JsonPropertyName("geometry")]
    [Description("Raw GeoJSON geometry payload as stored by the API. Supports Point, LineString, Polygon, and other GeoJSON geometry objects.")]
    public JsonNode? Geometry { get; init; }

    /// <summary>
    /// Gets or sets the application-specific feature metadata.
    /// </summary>
    [JsonPropertyName("properties")]
    [Description("Application-specific metadata associated with the feature.")]
    public GeoJsonFeaturePropertiesResponse Properties { get; init; } = new();
}

/// <summary>
/// Represents the properties block inside a GeoJSON feature response.
/// </summary>
public sealed class GeoJsonFeaturePropertiesResponse
{
    /// <summary>
    /// Gets or sets the unique feature identifier.
    /// </summary>
    [Description("Unique identifier of the feature.")]
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the display name shown in the map UI.
    /// </summary>
    [Description("Display name shown in the map UI.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description shown in the details panel.
    /// </summary>
    [Description("Optional textual description shown in the map details panel.")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the GeoJSON geometry type.
    /// </summary>
    [Description("Geometry type resolved from the GeoJSON payload, for example Point or Polygon.")]
    public string GeometryType { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin of the feature.
    /// </summary>
    [Description("Origin of the feature, for example user-created or seeded data.")]
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    [Description("Feature creation timestamp in UTC.")]
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets or sets the last update timestamp in UTC.
    /// </summary>
    [Description("Feature last update timestamp in UTC.")]
    public DateTime UpdatedAtUtc { get; init; }
}