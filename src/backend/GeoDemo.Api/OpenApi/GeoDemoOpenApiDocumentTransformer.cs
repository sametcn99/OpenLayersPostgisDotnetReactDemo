using System.Text.Json.Nodes;
using GeoDemo.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace GeoDemo.Api.OpenApi;

/// <summary>
/// Enriches the generated OpenAPI document with descriptions and concrete examples for Scalar.
/// </summary>
internal static class GeoDemoOpenApiDocumentTransformer
{
    public static Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext _, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        document.Info = new OpenApiInfo
        {
            Title = "OpenLayersDotnetTest API",
            Version = "v1",
            Description = "Spatial demo API for storing and retrieving GeoJSON map features backed by PostGIS. The API returns GeoJSON-compatible responses for frontend map rendering.",
        };

        ApplyGeometrySchemas(document);

        ApplySchemaExample(document, nameof(HealthStatusResponse), ExampleHealthResponse());
        ApplySchemaExample(document, nameof(SaveMapFeatureRequest), ExampleSaveFeatureRequest());
        ApplySchemaExample(document, nameof(GeoJsonFeatureResponse), ExampleFeatureResponse());
        ApplySchemaExample(document, nameof(GeoJsonFeatureCollectionResponse), ExampleFeatureCollectionResponse());
        ApplySchemaExample(document, nameof(ProblemDetails), ExampleNotFoundProblem());
        ApplySchemaExample(document, nameof(ValidationProblemDetails), ExampleInvalidGeometryProblem());

        ApplyRequestExample(document, "/api/map-features", HttpMethod.Post, ExampleSaveFeatureRequest());
        ApplyRequestExample(document, "/api/map-features/{id}", HttpMethod.Put, ExampleUpdateFeatureRequest());

        ApplyResponseExample(document, "/api/health", HttpMethod.Get, StatusCodes.Status200OK, "application/json", ExampleHealthResponse());
        ApplyResponseExample(document, "/api/map-features", HttpMethod.Get, StatusCodes.Status200OK, "application/json", ExampleFeatureCollectionResponse());
        ApplyResponseExample(document, "/api/map-features", HttpMethod.Post, StatusCodes.Status201Created, "application/json", ExampleFeatureResponse());
        NormalizeProblemResponseContent(document, "/api/map-features", HttpMethod.Post, StatusCodes.Status400BadRequest);
        NormalizeProblemResponseContent(document, "/api/map-features/{id}", HttpMethod.Get, StatusCodes.Status404NotFound);
        NormalizeProblemResponseContent(document, "/api/map-features/{id}", HttpMethod.Put, StatusCodes.Status400BadRequest);
        NormalizeProblemResponseContent(document, "/api/map-features/{id}", HttpMethod.Put, StatusCodes.Status404NotFound);
        NormalizeProblemResponseContent(document, "/api/map-features/{id}", HttpMethod.Delete, StatusCodes.Status404NotFound);

        ApplyResponseExample(document, "/api/map-features", HttpMethod.Post, StatusCodes.Status400BadRequest, "application/problem+json", ExampleInvalidGeometryProblem());
        ApplyResponseExample(document, "/api/map-features/{id}", HttpMethod.Get, StatusCodes.Status200OK, "application/json", ExampleFeatureResponse());
        ApplyResponseExample(document, "/api/map-features/{id}", HttpMethod.Get, StatusCodes.Status404NotFound, "application/problem+json", ExampleNotFoundProblem());
        ApplyResponseExample(document, "/api/map-features/{id}", HttpMethod.Put, StatusCodes.Status200OK, "application/json", ExampleUpdatedFeatureResponse());
        ApplyResponseExample(document, "/api/map-features/{id}", HttpMethod.Put, StatusCodes.Status400BadRequest, "application/problem+json", ExampleInvalidGeometryProblem());
        ApplyResponseExample(document, "/api/map-features/{id}", HttpMethod.Put, StatusCodes.Status404NotFound, "application/problem+json", ExampleNotFoundProblem());
        ApplyResponseExample(document, "/api/map-features/{id}", HttpMethod.Delete, StatusCodes.Status404NotFound, "application/problem+json", ExampleNotFoundProblem());

        return Task.CompletedTask;
    }

      private static void ApplyGeometrySchemas(OpenApiDocument document)
      {
        OverridePropertySchema(document, nameof(SaveMapFeatureRequest), "geometry", CreateGeoJsonGeometrySchema());
        OverridePropertySchema(document, nameof(GeoJsonFeatureResponse), "geometry", CreateGeoJsonGeometrySchema());
      }

      private static void OverridePropertySchema(OpenApiDocument document, string schemaName, string propertyName, OpenApiSchema propertySchema)
      {
        if (document.Components?.Schemas is null ||
          !document.Components.Schemas.TryGetValue(schemaName, out var schema) ||
          schema is not OpenApiSchema concreteSchema ||
          concreteSchema.Properties is null)
        {
          return;
        }

        concreteSchema.Properties[propertyName] = propertySchema;
      }

    private static void ApplySchemaExample(OpenApiDocument document, string schemaName, JsonNode example)
    {
        if (document.Components?.Schemas is not null && document.Components.Schemas.TryGetValue(schemaName, out var schema) && schema is OpenApiSchema concreteSchema)
        {
            concreteSchema.Example = example.DeepClone();
        }
    }

    private static void ApplyRequestExample(OpenApiDocument document, string path, HttpMethod operationType, JsonNode example)
    {
        if (!TryGetOperation(document, path, operationType, out var operation) || operation.RequestBody is null)
        {
            return;
        }

        var requestBody = operation.RequestBody;
        if (requestBody.Content is not null && requestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example.DeepClone();
        }
    }

    private static void ApplyResponseExample(OpenApiDocument document, string path, HttpMethod operationType, int statusCode, string mediaTypeName, JsonNode example)
    {
        if (!TryGetOperation(document, path, operationType, out var operation))
        {
            return;
        }

        var responses = operation.Responses;
        if (responses is null || !responses.TryGetValue(statusCode.ToString(), out var response))
        {
            return;
        }

        if (response.Content is not null && response.Content.TryGetValue(mediaTypeName, out var mediaType))
        {
            mediaType.Example = example.DeepClone();
        }
    }

    private static void NormalizeProblemResponseContent(OpenApiDocument document, string path, HttpMethod operationType, int statusCode)
    {
        if (!TryGetOperation(document, path, operationType, out var operation))
        {
            return;
        }

        var responses = operation.Responses;
        if (responses is null || !responses.TryGetValue(statusCode.ToString(), out var response) || response.Content is null)
        {
            return;
        }

        if (response.Content.ContainsKey("application/problem+json"))
        {
            return;
        }

        if (response.Content.TryGetValue("application/json", out var jsonMediaType))
        {
            response.Content["application/problem+json"] = new OpenApiMediaType(jsonMediaType);
            response.Content.Remove("application/json");
        }
    }

    private static bool TryGetOperation(OpenApiDocument document, string path, HttpMethod operationType, out OpenApiOperation operation)
    {
        operation = null!;

        if (!document.Paths.TryGetValue(path, out var pathItem))
        {
            return false;
        }

        var operations = pathItem.Operations;
        if (operations is null || !operations.TryGetValue(operationType, out var foundOperation))
        {
          return false;
        }

        operation = foundOperation;
        return true;
    }

      private static OpenApiSchema CreateGeoJsonGeometrySchema()
      {
        return new OpenApiSchema
        {
          Type = JsonSchemaType.Object,
          Description = "GeoJSON geometry object. Choose the variant that matches the `type` field. Scalar will show the required fields and coordinate nesting for each supported geometry shape.",
          OneOf = new List<IOpenApiSchema>
          {
            CreatePointGeometrySchema(),
            CreateLineStringGeometrySchema(),
            CreatePolygonGeometrySchema(),
            CreateMultiPointGeometrySchema(),
            CreateMultiLineStringGeometrySchema(),
            CreateMultiPolygonGeometrySchema(),
            CreateGeometryCollectionSchema(),
          },
          Example = ExamplePointGeometry(),
        };
      }

      private static OpenApiSchema CreatePointGeometrySchema()
      {
        return CreateGeometryObjectSchema(
          "Point geometry",
          "A GeoJSON Point uses a single position array in the form `[longitude, latitude]` or `[longitude, latitude, altitude]`.",
          "Point",
          CreatePositionSchema(),
          ExamplePointGeometry());
      }

      private static OpenApiSchema CreateLineStringGeometrySchema()
      {
        return CreateGeometryObjectSchema(
          "LineString geometry",
          "A GeoJSON LineString uses an array of two or more positions.",
          "LineString",
          CreateArraySchema(
            "Ordered list of positions that make up the line.",
            CreatePositionSchema(),
            ExampleLineStringCoordinates()),
          ExampleLineStringGeometry());
      }

      private static OpenApiSchema CreatePolygonGeometrySchema()
      {
        return CreateGeometryObjectSchema(
          "Polygon geometry",
          "A GeoJSON Polygon uses an array of linear rings. The first ring is the outer boundary and any following rings represent holes.",
          "Polygon",
          CreateArraySchema(
            "Array of linear rings. Each ring is an array of positions.",
            CreateArraySchema(
              "Linear ring represented as an array of positions.",
              CreatePositionSchema(),
              ExamplePolygonRing()),
            ExamplePolygonCoordinates()),
          ExamplePolygonGeometry());
      }

      private static OpenApiSchema CreateMultiPointGeometrySchema()
      {
        return CreateGeometryObjectSchema(
          "MultiPoint geometry",
          "A GeoJSON MultiPoint uses an array of positions.",
          "MultiPoint",
          CreateArraySchema(
            "Array of positions.",
            CreatePositionSchema(),
            ExampleMultiPointCoordinates()),
          ExampleMultiPointGeometry());
      }

      private static OpenApiSchema CreateMultiLineStringGeometrySchema()
      {
        return CreateGeometryObjectSchema(
          "MultiLineString geometry",
          "A GeoJSON MultiLineString uses an array of line strings, each line string being an array of positions.",
          "MultiLineString",
          CreateArraySchema(
            "Array of line strings.",
            CreateArraySchema(
              "Single line string represented as an array of positions.",
              CreatePositionSchema(),
              ExampleLineStringCoordinates()),
            ExampleMultiLineStringCoordinates()),
          ExampleMultiLineStringGeometry());
      }

      private static OpenApiSchema CreateMultiPolygonGeometrySchema()
      {
        return CreateGeometryObjectSchema(
          "MultiPolygon geometry",
          "A GeoJSON MultiPolygon uses an array of polygons. Each polygon is an array of linear rings.",
          "MultiPolygon",
          CreateArraySchema(
            "Array of polygons.",
            CreateArraySchema(
              "Single polygon represented as an array of linear rings.",
              CreateArraySchema(
                "Linear ring represented as an array of positions.",
                CreatePositionSchema(),
                ExamplePolygonRing()),
              ExamplePolygonCoordinates()),
            ExampleMultiPolygonCoordinates()),
          ExampleMultiPolygonGeometry());
      }

      private static OpenApiSchema CreateGeometryCollectionSchema()
      {
        return new OpenApiSchema
        {
          Type = JsonSchemaType.Object,
          Title = "GeometryCollection geometry",
          Description = "A GeoJSON GeometryCollection uses a `geometries` array instead of `coordinates`.",
          Properties = new Dictionary<string, IOpenApiSchema>
          {
            ["type"] = CreateTypePropertySchema("GeometryCollection"),
            ["geometries"] = CreateArraySchema(
              "Array of nested GeoJSON geometries.",
              CreateNestedGeometrySchema(),
              ExampleGeometryCollectionItems()),
            ["bbox"] = CreateBoundingBoxSchema(),
          },
          Required = new HashSet<string> { "type", "geometries" },
          Example = ExampleGeometryCollectionGeometry(),
        };
      }

      private static OpenApiSchema CreateNestedGeometrySchema()
      {
        return new OpenApiSchema
        {
          Description = "Nested geometry inside a GeometryCollection.",
          OneOf = new List<IOpenApiSchema>
          {
            CreatePointGeometrySchema(),
            CreateLineStringGeometrySchema(),
            CreatePolygonGeometrySchema(),
            CreateMultiPointGeometrySchema(),
            CreateMultiLineStringGeometrySchema(),
            CreateMultiPolygonGeometrySchema(),
          },
          Example = ExamplePointGeometry(),
        };
      }

      private static OpenApiSchema CreateGeometryObjectSchema(string title, string description, string geometryType, OpenApiSchema coordinatesSchema, JsonNode example)
      {
        return new OpenApiSchema
        {
          Type = JsonSchemaType.Object,
          Title = title,
          Description = description,
          Properties = new Dictionary<string, IOpenApiSchema>
          {
            ["type"] = CreateTypePropertySchema(geometryType),
            ["coordinates"] = coordinatesSchema,
            ["bbox"] = CreateBoundingBoxSchema(),
          },
          Required = new HashSet<string> { "type", "coordinates" },
          Example = example,
        };
      }

      private static OpenApiSchema CreateTypePropertySchema(string geometryType)
      {
        return new OpenApiSchema
        {
          Type = JsonSchemaType.String,
          Description = $"GeoJSON geometry type. Use `{geometryType}`.",
          Example = JsonValue.Create(geometryType),
        };
      }

      private static OpenApiSchema CreatePositionSchema()
      {
        return CreateArraySchema(
          "Single geographic position written as `[longitude, latitude]` or `[longitude, latitude, altitude]`.",
          new OpenApiSchema
          {
            Type = JsonSchemaType.Number,
            Description = "Coordinate component as a decimal number.",
            Example = JsonValue.Create(29.0265),
          },
          ExamplePosition());
      }

      private static OpenApiSchema CreateBoundingBoxSchema()
      {
        return CreateArraySchema(
          "Optional GeoJSON bounding box. Commonly `[minLon, minLat, maxLon, maxLat]`.",
          new OpenApiSchema
          {
            Type = JsonSchemaType.Number,
            Description = "Bounding box coordinate component as a decimal number.",
            Example = JsonValue.Create(29.0265),
          },
          ExampleBoundingBox());
      }

      private static OpenApiSchema CreateArraySchema(string description, OpenApiSchema itemsSchema, JsonNode example)
      {
        return new OpenApiSchema
        {
          Type = JsonSchemaType.Array,
          Description = description,
          Items = itemsSchema,
          Example = example,
        };
      }

    private static JsonNode ExampleHealthResponse()
    {
        return JsonNode.Parse("""
        {
          "status": "ok"
        }
        """)!;
    }

    private static JsonNode ExampleSaveFeatureRequest()
    {
        return JsonNode.Parse("""
        {
          "name": "Kadikoy Ferry Pier",
          "description": "User-drawn point near the ferry terminal.",
          "geometry": {
            "type": "Point",
            "coordinates": [29.0265, 40.9919]
          }
        }
        """)!;
    }

    private static JsonNode ExamplePointGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "Point",
          "coordinates": [29.0265, 40.9919]
        }
        """)!;
    }

    private static JsonNode ExampleLineStringGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "LineString",
          "coordinates": [[29.0265, 40.9919], [29.0312, 40.9941], [29.0368, 40.9974]]
        }
        """)!;
    }

    private static JsonNode ExamplePolygonGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "Polygon",
          "coordinates": [
            [[29.0200, 40.9900], [29.0400, 40.9900], [29.0400, 41.0000], [29.0200, 41.0000], [29.0200, 40.9900]]
          ]
        }
        """)!;
    }

    private static JsonNode ExampleMultiPointGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "MultiPoint",
          "coordinates": [[29.0265, 40.9919], [29.0312, 40.9941]]
        }
        """)!;
    }

    private static JsonNode ExampleMultiLineStringGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "MultiLineString",
          "coordinates": [
            [[29.0265, 40.9919], [29.0312, 40.9941]],
            [[29.0330, 40.9950], [29.0368, 40.9974]]
          ]
        }
        """)!;
    }

    private static JsonNode ExampleMultiPolygonGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "MultiPolygon",
          "coordinates": [
            [
              [[29.0200, 40.9900], [29.0300, 40.9900], [29.0300, 40.9960], [29.0200, 40.9960], [29.0200, 40.9900]]
            ],
            [
              [[29.0320, 40.9920], [29.0400, 40.9920], [29.0400, 40.9980], [29.0320, 40.9980], [29.0320, 40.9920]]
            ]
          ]
        }
        """)!;
    }

    private static JsonNode ExampleGeometryCollectionGeometry()
    {
        return JsonNode.Parse("""
        {
          "type": "GeometryCollection",
          "geometries": [
            {
              "type": "Point",
              "coordinates": [29.0265, 40.9919]
            },
            {
              "type": "LineString",
              "coordinates": [[29.0265, 40.9919], [29.0312, 40.9941]]
            }
          ]
        }
        """)!;
    }

    private static JsonNode ExamplePosition()
    {
        return JsonNode.Parse("""[29.0265, 40.9919]""")!;
    }

    private static JsonNode ExampleBoundingBox()
    {
        return JsonNode.Parse("""[29.0200, 40.9900, 29.0400, 41.0000]""")!;
    }

    private static JsonNode ExampleLineStringCoordinates()
    {
        return JsonNode.Parse("""[[29.0265, 40.9919], [29.0312, 40.9941], [29.0368, 40.9974]]""")!;
    }

    private static JsonNode ExamplePolygonRing()
    {
        return JsonNode.Parse("""[[29.0200, 40.9900], [29.0400, 40.9900], [29.0400, 41.0000], [29.0200, 41.0000], [29.0200, 40.9900]]""")!;
    }

    private static JsonNode ExamplePolygonCoordinates()
    {
        return JsonNode.Parse("""[[[29.0200, 40.9900], [29.0400, 40.9900], [29.0400, 41.0000], [29.0200, 41.0000], [29.0200, 40.9900]]]""")!;
    }

    private static JsonNode ExampleMultiPointCoordinates()
    {
        return JsonNode.Parse("""[[29.0265, 40.9919], [29.0312, 40.9941]]""")!;
    }

    private static JsonNode ExampleMultiLineStringCoordinates()
    {
        return JsonNode.Parse("""[[[29.0265, 40.9919], [29.0312, 40.9941]], [[29.0330, 40.9950], [29.0368, 40.9974]]]""")!;
    }

    private static JsonNode ExampleMultiPolygonCoordinates()
    {
        return JsonNode.Parse("""[[[[29.0200, 40.9900], [29.0300, 40.9900], [29.0300, 40.9960], [29.0200, 40.9960], [29.0200, 40.9900]]], [[[29.0320, 40.9920], [29.0400, 40.9920], [29.0400, 40.9980], [29.0320, 40.9980], [29.0320, 40.9920]]]]""")!;
    }

    private static JsonNode ExampleGeometryCollectionItems()
    {
        return JsonNode.Parse("""
        [
          {
            "type": "Point",
            "coordinates": [29.0265, 40.9919]
          },
          {
            "type": "LineString",
            "coordinates": [[29.0265, 40.9919], [29.0312, 40.9941]]
          }
        ]
        """)!;
    }

    private static JsonNode ExampleUpdateFeatureRequest()
    {
        return JsonNode.Parse("""
        {
          "name": "Kadikoy Ferry Pier Updated",
          "description": "Adjusted point after manual correction.",
          "geometry": {
            "type": "Point",
            "coordinates": [29.0271, 40.9924]
          }
        }
        """)!;
    }

    private static JsonNode ExampleFeatureCollectionResponse()
    {
        return JsonNode.Parse("""
        {
          "type": "FeatureCollection",
          "features": [
            {
              "type": "Feature",
              "id": "8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c",
              "geometry": {
                "type": "Point",
                "coordinates": [29.0265, 40.9919]
              },
              "properties": {
                "id": "8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c",
                "name": "Kadikoy Ferry Pier",
                "description": "User-drawn point near the ferry terminal.",
                "geometryType": "Point",
                "source": "User",
                "createdAtUtc": "2026-04-27T10:00:00Z",
                "updatedAtUtc": "2026-04-27T10:00:00Z"
              }
            }
          ]
        }
        """)!;
    }

    private static JsonNode ExampleFeatureResponse()
    {
        return JsonNode.Parse("""
        {
          "type": "Feature",
          "id": "8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c",
          "geometry": {
            "type": "Point",
            "coordinates": [29.0265, 40.9919]
          },
          "properties": {
            "id": "8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c",
            "name": "Kadikoy Ferry Pier",
            "description": "User-drawn point near the ferry terminal.",
            "geometryType": "Point",
            "source": "User",
            "createdAtUtc": "2026-04-27T10:00:00Z",
            "updatedAtUtc": "2026-04-27T10:00:00Z"
          }
        }
        """)!;
    }

    private static JsonNode ExampleUpdatedFeatureResponse()
    {
        return JsonNode.Parse("""
        {
          "type": "Feature",
          "id": "8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c",
          "geometry": {
            "type": "Point",
            "coordinates": [29.0271, 40.9924]
          },
          "properties": {
            "id": "8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c",
            "name": "Kadikoy Ferry Pier Updated",
            "description": "Adjusted point after manual correction.",
            "geometryType": "Point",
            "source": "User",
            "createdAtUtc": "2026-04-27T10:00:00Z",
            "updatedAtUtc": "2026-04-27T10:05:00Z"
          }
        }
        """)!;
    }

    private static JsonNode ExampleInvalidGeometryProblem()
    {
        return JsonNode.Parse("""
        {
          "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
          "title": "Invalid GeoJSON geometry",
          "status": 400,
          "detail": "The provided GeoJSON geometry payload could not be parsed or validated.",
          "errors": {
            "geometry": [
              "The GeoJSON geometry type 'CircularString' is not supported."
            ]
          }
        }
        """)!;
    }

    private static JsonNode ExampleNotFoundProblem()
    {
        return JsonNode.Parse("""
        {
          "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
          "title": "Map feature not found",
          "status": 404,
          "detail": "No map feature exists with id '8f8c0f13-7a07-4c67-8ee1-79d3df0f6c4c'."
        }
        """)!;
    }
}