namespace GeoDemo.Application.DTOs;

/// <summary>
/// Represents the information needed to create or update a map feature.
/// </summary>
public sealed record SaveMapFeatureInput(string Name, string? Description, string GeometryJson);