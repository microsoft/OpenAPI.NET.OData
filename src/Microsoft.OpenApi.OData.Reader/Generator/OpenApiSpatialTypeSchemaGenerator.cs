// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Create <see cref="OpenApiSchema"/> for Edm spatial types.
    /// </summary>
    internal static class OpenApiSpatialTypeSchemaGenerator
    {
        /// <summary>
        /// Create the dictionary of <see cref="OpenApiSchema"/> object.
        /// The name of each pair is the namespace-qualified name of the type. It uses the namespace instead of the alias.
        /// The value of each pair is a <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <param name="document">The document to use to lookup references.</param>
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, IOpenApiSchema> CreateSpatialSchemas(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            var schemas = new Dictionary<string, IOpenApiSchema>();

            if (context.IsSpatialTypeUsed)
            {
                schemas.Add("Edm.Geography", CreateEdmGeographySchema(document));

                schemas.Add("Edm.GeographyPoint", CreateEdmGeographyPointSchema(document));

                schemas.Add("Edm.GeographyLineString", CreateEdmGeographyLineStringSchema(document));

                schemas.Add("Edm.GeographyPolygon", CreateEdmGeographyPolygonSchema(document));

                schemas.Add("Edm.GeographyMultiPoint", CreateEdmGeographyMultiPointSchema(document));

                schemas.Add("Edm.GeographyMultiLineString", CreateEdmGeographyMultiLineStringSchema(document));

                schemas.Add("Edm.GeographyMultiPolygon", CreateEdmGeographyMultiPolygonSchema(document));

                schemas.Add("Edm.GeographyCollection", CreateEdmGeographyCollectionSchema(document));

                schemas.Add("Edm.Geometry", CreateEdmGeometrySchema(document));

                schemas.Add("Edm.GeometryPoint", CreateEdmGeometryPointSchema(document));

                schemas.Add("Edm.GeometryLineString", CreateEdmGeometryLineStringSchema(document));

                schemas.Add("Edm.GeometryPolygon", CreateEdmGeometryPolygonSchema(document));

                schemas.Add("Edm.GeometryMultiPoint", CreateEdmGeometryMultiPointSchema(document));

                schemas.Add("Edm.GeometryMultiLineString", CreateEdmGeometryMultiLineStringSchema(document));

                schemas.Add("Edm.GeometryMultiPolygon", CreateEdmGeometryMultiPolygonSchema(document));

                schemas.Add("Edm.GeometryCollection", CreateEdmGeometryCollectionSchema(document));

                schemas.Add("GeoJSON.position", CreateGeoJsonPointSchema());
            }

            return schemas;
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.Geography.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographySchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.Geometry", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyPointSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryPoint", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyLineStringSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryLineString", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyPolygonSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryPolygon", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyMultiPointSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryMultiPoint", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyMultiLineStringSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryMultiLineString", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyMultiPolygonSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryMultiPolygon", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyCollection.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static IOpenApiSchema CreateEdmGeographyCollectionSchema(OpenApiDocument document)
        {
            return new OpenApiSchemaReference("Edm.GeometryCollection", document);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.Geometry.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometrySchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                OneOf =
                [
                    new OpenApiSchemaReference("Edm.GeometryPoint", document),
                    new OpenApiSchemaReference("Edm.GeometryLineString", document),
                    new OpenApiSchemaReference("Edm.GeometryPolygon", document),
                    new OpenApiSchemaReference("Edm.GeometryMultiPoint", document),
                    new OpenApiSchemaReference("Edm.GeometryMultiLineString", document),
                    new OpenApiSchemaReference("Edm.GeometryMultiPolygon", document),
                    new OpenApiSchemaReference("Edm.GeometryCollection", document),
                ]
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryPointSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Enum = new List<JsonNode>
                            {
                                "Point"
                            },
                            Default = "Point"
                        }
                    },
                    { "coordinates", new OpenApiSchemaReference("GeoJSON.position", document) } 
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryLineStringSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<JsonNode>
                            {
                                "LineString"
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchemaReference("GeoJSON.position", document),
                            MinItems = 2
                        }
                    }
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryPolygonSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<JsonNode>
                            {
                                "Polygon"
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchemaReference("GeoJSON.position", document)
                            },
                            MinItems = 4
                        }
                    }
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryMultiPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryMultiPointSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<JsonNode>
                            {
                                "MultiPoint"
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchemaReference("GeoJSON.position", document)
                        }
                    }
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryMultiLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryMultiLineStringSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<JsonNode>
                            {
                                "MultiLineString"
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchemaReference("GeoJSON.position", document)
                            },
                            MinItems = 2
                        }
                    }
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryMultiPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryMultiPolygonSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<JsonNode>
                            {
                                "MultiPolygon"
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema
                            {
                                Type = JsonSchemaType.Array,
                                Items = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Array,
                                    Items = new OpenApiSchemaReference("GeoJSON.position", document)
                                }
                            },
                            MinItems = 4
                        }
                    }
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryCollection.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="document">The document to use to lookup references.</param>
        public static OpenApiSchema CreateEdmGeometryCollectionSchema(OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<JsonNode>
                            {
                                "GeometryCollection"
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchemaReference("Edm.Geometry", document)
                        }
                    }
                },
                Required = new HashSet<string>
                {
                    "type",
                    "coordinates"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for GeoJSON.position.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateGeoJsonPointSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = new OpenApiSchema { Type = JsonSchemaType.Number },
                MinItems = 2
            };
        }
    }
}
