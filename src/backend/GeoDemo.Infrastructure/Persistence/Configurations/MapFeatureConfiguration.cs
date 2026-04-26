using GeoDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeoDemo.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures PostGIS storage for map features.
/// </summary>
public sealed class MapFeatureConfiguration : IEntityTypeConfiguration<MapFeature>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MapFeature> builder)
    {
        builder.ToTable("map_features");

        builder.HasKey(feature => feature.Id);

        builder.Property(feature => feature.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(feature => feature.Description)
            .HasMaxLength(500);

        builder.Property(feature => feature.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(feature => feature.CreatedAtUtc)
            .IsRequired();

        builder.Property(feature => feature.UpdatedAtUtc)
            .IsRequired();

        builder.Property(feature => feature.Geometry)
            .HasColumnType("geometry(Geometry,4326)")
            .IsRequired();

        builder.HasIndex(feature => feature.Geometry)
            .HasMethod("gist");
    }
}