// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

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
            Utils.CheckArgumentNull(context, nameof(context));

            return new Dictionary<string, OpenApiSchema>()
            {
                // odata.error
                { "odata.error", CreateErrorSchema() },

                // odata.error.main
                { "odata.error.main", CreateErrorMainSchema() },

                // odata.error.detail
                { "odata.error.detail", CreateErrorDetailSchema() },

                // odata.error.innererror
                { "odata.error.innererror", CreateInnerErrorSchema(context) }
            };
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
                Required = new HashSet<string>
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
        /// Creates the inner error schema definition. If an "InnerError" complex type is defined in the root namespace, then this type will be used as the inner error type.
        /// Otherwise, a default inner error type of object will be created.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The inner error schema definition.</returns>
        public static OpenApiSchema CreateInnerErrorSchema(ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            var rootNamespace = context.Model.DeclaredNamespaces.OrderBy(n => n.Count(x => x == '.')).FirstOrDefault();
            if(!string.IsNullOrEmpty(context.Settings.InnerErrorComplexTypeName) &&
                !string.IsNullOrEmpty(rootNamespace) &&
                context.Model.FindDeclaredType($"{rootNamespace}.{context.Settings.InnerErrorComplexTypeName}") is IEdmComplexType complexType)
            {
                return context.CreateSchemaTypeSchema(complexType);
            }
            
            return new OpenApiSchema
            {
                Type = "object",
                Description = "The structure of this object is service-specific"
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
                Required = new HashSet<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string", Nullable = false }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string", Nullable = false, }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string", Nullable = true }
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
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "odata.error.innererror"
                            }
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
                Required = new HashSet<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string", Nullable = false, }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string", Nullable = false, }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string", Nullable = true, }
                    }
                }
            };
        }
    }
}
