using System.Text.Json;
using GeoDemo.Application.Abstractions;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace GeoDemo.Infrastructure.Services;

/// <summary>
/// Converts supported GeoJSON geometry payloads using System.Text.Json converters.
/// </summary>
public sealed class GeoJsonGeometryConverter : IGeometryGeoJsonConverter
{
    private const int SupportedSrid = 4326;

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    /// <inheritdoc />
    public Geometry Read(string geometryJson)
    {
        if (string.IsNullOrWhiteSpace(geometryJson))
        {
            throw new ArgumentException("Geometry payload is required.", nameof(geometryJson));
        }

        try
        {
            var geometry = JsonSerializer.Deserialize<Geometry>(geometryJson, SerializerOptions)
                ?? throw new ArgumentException("Geometry payload is invalid.", nameof(geometryJson));

            geometry.SRID = SupportedSrid;
            EnsureSupportedGeometry(geometry);
            return geometry;
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Geometry must be a valid GeoJSON geometry object.", nameof(geometryJson), exception);
        }
    }

    /// <inheritdoc />
    public string Write(Geometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        EnsureSupportedGeometry(geometry);
        return JsonSerializer.Serialize(geometry, SerializerOptions);
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: SupportedSrid);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new GeoJsonConverterFactory(geometryFactory));
        return options;
    }

    private static void EnsureSupportedGeometry(Geometry geometry)
    {
        if (geometry.IsEmpty)
        {
            throw new ArgumentException("Geometry cannot be empty.", nameof(geometry));
        }

        if (geometry is not Point && geometry is not LineString && geometry is not Polygon)
        {
            throw new ArgumentException("Only Point, LineString, and Polygon geometries are supported.", nameof(geometry));
        }
    }
}