namespace GeoDemo.Application.DTOs;

/// <summary>
/// Represents a feature returned to API callers.
/// </summary>
public sealed record MapFeatureDto(
    Guid Id,
    string Name,
    string? Description,
    string GeometryJson,
    string GeometryType,
    string Source,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);