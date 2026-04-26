using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json;

namespace GeoDemo.Api.Contracts;

/// <summary>
/// Represents the request payload for creating or updating a map feature.
/// </summary>
public sealed class SaveMapFeatureRequest
{
    /// <summary>
    /// Gets or sets the feature name shown in the UI.
    /// </summary>
    [Required]
    [StringLength(120, MinimumLength = 2)]
    [Description("Human-readable feature name shown in the map sidebar and popups.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description shown in the details panel.
    /// </summary>
    [StringLength(500)]
    [Description("Optional details about the feature. This value is stored as part of the feature metadata.")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the GeoJSON geometry payload.
    /// </summary>
    [Required]
    [Description("GeoJSON geometry object. Example: { \"type\": \"Point\", \"coordinates\": [29.0, 41.0] }.")]
    public JsonElement Geometry { get; init; }
}