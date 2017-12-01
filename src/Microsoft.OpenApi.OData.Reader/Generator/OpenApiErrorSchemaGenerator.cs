// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Create <see cref="OpenApiSchema"/> for Edm spatial types.
    /// </summary>
    internal static class OpenApiErrorSchemaGenerator
    {
        /// <summary>
        /// Create the dictionary of <see cref="OpenApiSchema"/> object.
        /// The name of each pair is the namespace-qualified name of the type. It uses the namespace instead of the alias.
        /// The value of each pair is a <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, OpenApiSchema> CreateODataErrorSchemas(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IDictionary<string, OpenApiSchema> schemas = new Dictionary<string, OpenApiSchema>();

            // odata.error
            schemas.Add("odata.error", CreateErrorSchema());

            // odata.error.main
            schemas.Add("odata.error.main", CreateErrorMainSchema());

            // odata.error.detail
            schemas.Add("odata.error.detail", CreateErrorDetailSchema());

            return schemas;
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for "odata.error".
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateErrorSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "error"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "error",
                        new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "odata.error.main"
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for "odata.error.main".
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateErrorMainSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "details",
                        new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "odata.error.detail"
                                }
                            }
                        }
                    },
                    {
                        "innererror",
                        new OpenApiSchema
                        {
                            Type = "object",
                            Description = "The structure of this object is service-specific"
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for "odata.error.detail".
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateErrorDetailSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string" }
                    }
                }
            };
        }
    }
}
