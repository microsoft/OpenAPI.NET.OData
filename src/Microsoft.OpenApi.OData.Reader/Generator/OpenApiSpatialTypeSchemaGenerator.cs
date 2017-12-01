// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

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
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, OpenApiSchema> CreateSpatialSchemas(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

            if (context.IsSpatialTypeUsed)
            {
                schemas.Add("Edm.Geography", CreateEdmGeographySchema());

                schemas.Add("Edm.GeographyPoint", CreateEdmGeographyPointSchema());

                schemas.Add("Edm.GeographyLineString", CreateEdmGeographyLineStringSchema());

                schemas.Add("Edm.GeographyPolygon", CreateEdmGeographyPolygonSchema());

                schemas.Add("Edm.GeographyMultiPoint", CreateEdmGeographyMultiPointSchema());

                schemas.Add("Edm.GeographyMultiLineString", CreateEdmGeographyMultiLineStringSchema());

                schemas.Add("Edm.GeographyMultiPolygon", CreateEdmGeographyMultiPolygonSchema());

                schemas.Add("Edm.GeographyCollection", CreateEdmGeographyCollectionSchema());

                schemas.Add("Edm.Geometry", CreateEdmGeometrySchema());

                schemas.Add("Edm.GeometryPoint", CreateEdmGeometryPointSchema());

                schemas.Add("Edm.GeometryLineString", CreateEdmGeometryLineStringSchema());

                schemas.Add("Edm.GeometryPolygon", CreateEdmGeometryPolygonSchema());

                schemas.Add("Edm.GeometryMultiPoint", CreateEdmGeometryMultiPointSchema());

                schemas.Add("Edm.GeometryMultiLineString", CreateEdmGeometryMultiLineStringSchema());

                schemas.Add("Edm.GeometryMultiPolygon", CreateEdmGeometryMultiPolygonSchema());

                schemas.Add("Edm.GeometryCollection", CreateEdmGeometryCollectionSchema());

                schemas.Add("GeoJSON.position", CreateGeoJsonPointSchema());
            }

            return schemas;
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.Geography.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographySchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.Geometry"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyPointSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryPoint"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyLineStringSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryLineString"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyPolygonSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryPolygon"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyMultiPointSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryMultiPoint"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyMultiLineStringSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryMultiLineString"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyMultiPolygonSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryMultiPolygon"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyCollection.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyCollectionSchema()
        {
            return new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Edm.GeometryCollection"
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.Geometry.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeometrySchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                AnyOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryPoint" } },
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryLineString" } },
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryPolygon" } },
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryMultiPoint" } },
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryMultiLineString" } },
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryMultiPolygon" } },
                    new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.GeometryCollection" } }
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeometryPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeometryPointSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Type = "string",
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("Point")
                            },
                            Default = new OpenApiString("Point")
                        }
                    },
                    { "coordinates", new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GeoJSON.position" } } }
                },
                Required = new List<string>
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
        public static OpenApiSchema CreateEdmGeometryLineStringSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("LineString")
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GeoJSON.position" }},
                            MinItems = 2
                        }
                    }
                },
                Required = new List<string>
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
        public static OpenApiSchema CreateEdmGeometryPolygonSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("Polygon")
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GeoJSON.position" } }
                            },
                            MinItems = 4
                        }
                    }
                },
                Required = new List<string>
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
        public static OpenApiSchema CreateEdmGeometryMultiPointSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("MultiPoint")
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GeoJSON.position" }}
                        }
                    }
                },
                Required = new List<string>
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
        public static OpenApiSchema CreateEdmGeometryMultiLineStringSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("MultiLineString")
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GeoJSON.position" } }
                            },
                            MinItems = 2
                        }
                    }
                },
                Required = new List<string>
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
        public static OpenApiSchema CreateEdmGeometryMultiPolygonSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("MultiPolygon")
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = "array",
                                    Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "GeoJSON.position" } }
                                }
                            },
                            MinItems = 4
                        }
                    }
                },
                Required = new List<string>
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
        public static OpenApiSchema CreateEdmGeometryCollectionSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "type", new OpenApiSchema
                        {
                            Enum = new List<IOpenApiAny>
                            {
                                new OpenApiString("GeometryCollection")
                            },
                        }
                    },
                    { "coordinates", new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Edm.Geometry" } }
                        }
                    }
                },
                Required = new List<string>
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
                Type = "array",
                Items = new OpenApiSchema { Type = "number" },
                MinItems = 2
            };
        }
    }
}
