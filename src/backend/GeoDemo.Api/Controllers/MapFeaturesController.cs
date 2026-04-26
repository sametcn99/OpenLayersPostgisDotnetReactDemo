using GeoDemo.Api.Contracts;
using GeoDemo.Application.DTOs;
using GeoDemo.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace GeoDemo.Api.Controllers;

/// <summary>
/// Exposes CRUD operations for GeoJSON features shown on the demo map.
/// </summary>
[ApiController]
[Route("api/map-features")]
public sealed class MapFeaturesController(IMapFeatureService mapFeatureService) : ControllerBase
{
    /// <summary>
    /// Returns all stored features as a GeoJSON feature collection.
    /// </summary>
    /// <remarks>
    /// The response follows the GeoJSON FeatureCollection structure and includes application metadata in the properties object of each feature.
    /// </remarks>
    /// <response code="200">Returns the stored features as a GeoJSON feature collection.</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GeoJsonFeatureCollectionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GeoJsonFeatureCollectionResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var features = await mapFeatureService.ListAsync(cancellationToken);
        return Ok(GeoJsonResponseFactory.CreateFeatureCollection(features));
    }

    /// <summary>
    /// Returns a single GeoJSON feature by identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the requested feature.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <response code="200">Returns the requested GeoJSON feature.</response>
    /// <response code="404">Returned when no feature exists for the supplied identifier.</response>
    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GeoJsonFeatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeoJsonFeatureResponse>> GetByIdAsync(
        [Description("Unique identifier of the feature.")] Guid id,
        CancellationToken cancellationToken)
    {
        var feature = await mapFeatureService.GetByIdAsync(id, cancellationToken);
        return feature is null ? FeatureNotFound(id) : Ok(GeoJsonResponseFactory.CreateFeature(feature));
    }

    /// <summary>
    /// Creates a new user feature from a GeoJSON geometry payload.
    /// </summary>
    /// <remarks>
    /// Supply a valid GeoJSON geometry object in the request body. The created resource is returned as a GeoJSON feature.
    /// </remarks>
    /// <param name="request">Feature payload containing name, optional description, and GeoJSON geometry.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <response code="201">Returns the created GeoJSON feature.</response>
    /// <response code="400">Returned when the geometry payload is invalid.</response>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GeoJsonFeatureResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeoJsonFeatureResponse>> CreateAsync([FromBody] SaveMapFeatureRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var feature = await mapFeatureService.CreateAsync(Map(request), cancellationToken);
            return CreatedAtRoute(nameof(GetByIdAsync), new { id = feature.Id }, GeoJsonResponseFactory.CreateFeature(feature));
        }
        catch (ArgumentException exception)
        {
            return InvalidGeometry(exception.Message);
        }
    }

    /// <summary>
    /// Updates an existing user feature.
    /// </summary>
    /// <remarks>
    /// The full GeoJSON geometry is replaced by the payload supplied in the request body.
    /// </remarks>
    /// <param name="id">Unique identifier of the feature to update.</param>
    /// <param name="request">Updated feature payload containing name, optional description, and GeoJSON geometry.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <response code="200">Returns the updated GeoJSON feature.</response>
    /// <response code="400">Returned when the geometry payload is invalid.</response>
    /// <response code="404">Returned when the requested feature does not exist.</response>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GeoJsonFeatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeoJsonFeatureResponse>> UpdateAsync(
        [Description("Unique identifier of the feature.")] Guid id,
        [FromBody] SaveMapFeatureRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var feature = await mapFeatureService.UpdateAsync(id, Map(request), cancellationToken);
            return feature is null ? FeatureNotFound(id) : Ok(GeoJsonResponseFactory.CreateFeature(feature));
        }
        catch (ArgumentException exception)
        {
            return InvalidGeometry(exception.Message);
        }
    }

    /// <summary>
    /// Deletes a feature by identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the feature to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <response code="204">Returned when the feature is deleted successfully.</response>
    /// <response code="404">Returned when the requested feature does not exist.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        [Description("Unique identifier of the feature.")] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await mapFeatureService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : FeatureNotFound(id);
    }

    private static SaveMapFeatureInput Map(SaveMapFeatureRequest request)
    {
        return new SaveMapFeatureInput(request.Name, request.Description, request.Geometry.GetRawText());
    }

    private ActionResult FeatureNotFound(Guid id)
    {
        return new NotFoundObjectResult(ApiProblemDetailsFactory.CreateFeatureNotFound(id))
        {
            ContentTypes = { "application/problem+json" },
        };
    }

    private ActionResult InvalidGeometry(string message)
    {
        return new BadRequestObjectResult(ApiProblemDetailsFactory.CreateInvalidGeometry(message))
        {
            ContentTypes = { "application/problem+json" },
        };
    }
}