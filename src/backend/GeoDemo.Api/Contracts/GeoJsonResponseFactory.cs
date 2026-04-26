using System.Text.Json.Nodes;
using GeoDemo.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GeoDemo.Api.Contracts;

/// <summary>
/// Shapes application DTOs into GeoJSON feature responses for the frontend.
/// </summary>
public static class GeoJsonResponseFactory
{
    /// <summary>
    /// Creates a GeoJSON feature collection from feature DTOs.
    /// </summary>
    public static GeoJsonFeatureCollectionResponse CreateFeatureCollection(IReadOnlyList<MapFeatureDto> features)
    {
        return new GeoJsonFeatureCollectionResponse
        {
            Features = features.Select(CreateFeature).ToArray(),
        };
    }

    /// <summary>
    /// Creates a single GeoJSON feature from a feature DTO.
    /// </summary>
    public static GeoJsonFeatureResponse CreateFeature(MapFeatureDto feature)
    {
        return new GeoJsonFeatureResponse
        {
            Id = feature.Id,
            Geometry = JsonNode.Parse(feature.GeometryJson),
            Properties = new GeoJsonFeaturePropertiesResponse
            {
                Id = feature.Id,
                Name = feature.Name,
                Description = feature.Description,
                GeometryType = feature.GeometryType,
                Source = feature.Source,
                CreatedAtUtc = feature.CreatedAtUtc,
                UpdatedAtUtc = feature.UpdatedAtUtc,
            },
        };
    }

    /// <summary>
    /// Creates a validation problem response for invalid GeoJSON input.
    /// </summary>
    public static object CreateGeometryValidationProblem(string message)
    {
        return new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["geometry"] = [message],
        });
    }
}