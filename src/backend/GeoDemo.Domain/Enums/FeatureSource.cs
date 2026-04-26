namespace GeoDemo.Domain.Enums;

/// <summary>
/// Describes how a map feature entered the system.
/// </summary>
public enum FeatureSource
{
    /// <summary>
    /// Feature was inserted by the demo seed process.
    /// </summary>
    Seed = 1,

    /// <summary>
    /// Feature was created by a local demo user.
    /// </summary>
    User = 2,
}