using GeoDemo.Application.Abstractions;
using GeoDemo.Api.Contracts;
using GeoDemo.Api.OpenApi;
using GeoDemo.Application.Services;
using GeoDemo.Infrastructure.Persistence;
using GeoDemo.Infrastructure.Repositories;
using GeoDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("GeoDemoDatabase")
    ?? throw new InvalidOperationException("Connection string 'GeoDemoDatabase' is required.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options => { options.AddDocumentTransformer(GeoDemoOpenApiDocumentTransformer.TransformAsync); });

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentClient", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<GeoDemoDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));

builder.Services.AddScoped<GeoDemoDbInitializer>();
builder.Services.AddScoped<IMapFeatureRepository, MapFeatureRepository>();
builder.Services.AddScoped<IMapFeatureService, MapFeatureService>();
builder.Services.AddSingleton<IGeometryGeoJsonConverter, GeoJsonGeometryConverter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<GeoDemoDbInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar", options =>
    {
        options.WithTitle("OpenLayersDotnetTest Scalar API")
            .ExpandAllModelSections()
            .ExpandAllTags()
            .ExpandAllResponses();
    });
}

app.UseCors("DevelopmentClient");

app.MapGet("/api/health", () => Results.Ok(new HealthStatusResponse { Status = "ok" }))
    .WithName("Health")
    .WithTags("Health")
    .WithSummary("Checks whether the API is healthy.")
    .WithDescription("Lightweight health endpoint used to verify that the backend process is running and can accept requests.")
    .Produces<HealthStatusResponse>();

app.MapControllers();

app.Run();
