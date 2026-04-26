using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeoDemo.Infrastructure.Persistence;

/// <summary>
/// Creates the DbContext for design-time EF Core tooling.
/// </summary>
public sealed class GeoDemoDbContextFactory : IDesignTimeDbContextFactory<GeoDemoDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=geodemo;Username=postgres;Password=postgres";

    /// <inheritdoc />
    public GeoDemoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GeoDemoDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("GEODEMO_CONNECTION_STRING") ?? DefaultConnectionString;

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite());

        return new GeoDemoDbContext(optionsBuilder.Options);
    }
}