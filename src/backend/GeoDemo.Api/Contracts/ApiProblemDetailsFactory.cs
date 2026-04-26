using Microsoft.AspNetCore.Mvc;

namespace GeoDemo.Api.Contracts;

/// <summary>
/// Creates standard problem details payloads used by the API.
/// </summary>
public static class ApiProblemDetailsFactory
{
    /// <summary>
    /// Creates a validation problem response for invalid GeoJSON input.
    /// </summary>
    public static ValidationProblemDetails CreateInvalidGeometry(string message)
    {
        return new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["geometry"] = [message],
        })
        {
            Title = "Invalid GeoJSON geometry",
            Detail = "The provided GeoJSON geometry payload could not be parsed or validated.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
        };
    }

    /// <summary>
    /// Creates a not found problem response for a missing feature.
    /// </summary>
    public static ProblemDetails CreateFeatureNotFound(Guid id)
    {
        return new ProblemDetails
        {
            Title = "Map feature not found",
            Detail = $"No map feature exists with id '{id}'.",
            Status = StatusCodes.Status404NotFound,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
        };
    }
}