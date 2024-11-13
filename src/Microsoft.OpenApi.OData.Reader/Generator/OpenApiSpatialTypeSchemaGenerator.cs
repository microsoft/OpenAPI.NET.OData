// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
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
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, OpenApiSchema> CreateSpatialSchemas(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

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
            return new OpenApiSchemaReference("Edm.Geometry", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyPointSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryPoint", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyLineStringSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryLineString", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyPolygonSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryPolygon", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiPoint.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyMultiPointSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryMultiPoint", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiLineString.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyMultiLineStringSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryMultiLineString", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyMultiPolygon.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyMultiPolygonSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryMultiPolygon", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.GeographyCollection.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeographyCollectionSchema()
        {
            return new OpenApiSchemaReference("Edm.GeometryCollection", null);
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for Edm.Geometry.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateEdmGeometrySchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                OneOf =
                [
                    new OpenApiSchemaReference("Edm.GeometryPoint", null),
                    new OpenApiSchemaReference("Edm.GeometryLineString", null),
                    new OpenApiSchemaReference("Edm.GeometryPolygon", null),
                    new OpenApiSchemaReference("Edm.GeometryMultiPoint", null),
                    new OpenApiSchemaReference("Edm.GeometryMultiLineString", null),
                    new OpenApiSchemaReference("Edm.GeometryMultiPolygon", null),
                    new OpenApiSchemaReference("Edm.GeometryCollection", null)
                ]
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
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                    { "coordinates", new OpenApiSchemaReference("GeoJSON.position", null) } 
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
        public static OpenApiSchema CreateEdmGeometryLineStringSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                            Items = new OpenApiSchemaReference("GeoJSON.position", null),
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
        public static OpenApiSchema CreateEdmGeometryPolygonSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                                Items = new OpenApiSchemaReference("GeoJSON.position", null)
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
        public static OpenApiSchema CreateEdmGeometryMultiPointSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                            Items = new OpenApiSchemaReference("GeoJSON.position", null)
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
        public static OpenApiSchema CreateEdmGeometryMultiLineStringSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                                Items = new OpenApiSchemaReference("GeoJSON.position", null)
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
        public static OpenApiSchema CreateEdmGeometryMultiPolygonSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                                    Items = new OpenApiSchemaReference("GeoJSON.position", null)
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
        public static OpenApiSchema CreateEdmGeometryCollectionSchema()
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, OpenApiSchema>
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
                            Items = new OpenApiSchemaReference("Edm.Geometry", null)
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
