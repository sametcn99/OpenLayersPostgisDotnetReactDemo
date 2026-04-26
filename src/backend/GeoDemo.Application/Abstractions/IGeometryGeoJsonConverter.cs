using NetTopologySuite.Geometries;

namespace GeoDemo.Application.Abstractions;

/// <summary>
/// Converts between GeoJSON geometry payloads and NetTopologySuite geometries.
/// </summary>
public interface IGeometryGeoJsonConverter
{
    /// <summary>
    /// Parses a GeoJSON geometry payload into a geometry instance.
    /// </summary>
    Geometry Read(string geometryJson);

    /// <summary>
    /// Serializes a geometry instance into a GeoJSON geometry payload.
    /// </summary>
    string Write(Geometry geometry);
}