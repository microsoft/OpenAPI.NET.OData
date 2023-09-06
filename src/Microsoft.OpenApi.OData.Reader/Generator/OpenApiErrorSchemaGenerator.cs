// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Create <see cref="OpenApiSchema"/> for Edm spatial types.
    /// </summary>
    internal static class OpenApiErrorSchemaGenerator
    {
        internal const string ODataErrorClassName = "ODataError";
        internal const string MainErrorClassName = "MainError";
        internal const string ErrorDetailsClassName = "ErrorDetails";
        internal const string InnerErrorClassName = "InnerError";

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
            var rootNamespaceName = context.GetErrorNamespaceName();

            return new Dictionary<string, OpenApiSchema>()
            {
                { $"{rootNamespaceName}{ODataErrorClassName}", CreateErrorSchema(rootNamespaceName) },
                { $"{rootNamespaceName}{MainErrorClassName}", CreateErrorMainSchema(rootNamespaceName) },
                { $"{rootNamespaceName}{ErrorDetailsClassName}", CreateErrorDetailSchema() },
                { $"{rootNamespaceName}{InnerErrorClassName}", CreateInnerErrorSchema(context) }
            };
        }

        /// <summary>
        /// Gets the error namespace name based on the root namespace of the model.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The error namespace name.</returns>
        public static string GetErrorNamespaceName(this ODataContext context) {
            Utils.CheckArgumentNull(context, nameof(context));
            var rootNamespaceName = context.Model.DeclaredNamespaces.OrderBy(ns => ns.Count(y => y == '.')).FirstOrDefault();
            rootNamespaceName += (string.IsNullOrEmpty(rootNamespaceName) ? string.Empty : ".") +
                                "ODataErrors.";
            return rootNamespaceName;
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for the error.
        /// </summary>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        /// <param name="rootNamespaceName">The root namespace name. With a trailing dot.</param>
        public static OpenApiSchema CreateErrorSchema(string rootNamespaceName)
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
                            UnresolvedReference = true,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = $"{rootNamespaceName}{MainErrorClassName}"
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
        /// Create <see cref="OpenApiSchema"/> for main property of the error.
        /// </summary>
        /// <param name="rootNamespaceName">The root namespace name. With a trailing dot.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateErrorMainSchema(string rootNamespaceName)
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
                        "message", new OpenApiSchema { Type = "string", Nullable = false, Extensions = new Dictionary<string, IOpenApiExtension> 
                                                                                    { { OpenApiPrimaryErrorMessageExtension.Name, new OpenApiPrimaryErrorMessageExtension { IsPrimaryErrorMessage = true } } } }
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
                                UnresolvedReference = true,
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = $"{rootNamespaceName}{ErrorDetailsClassName}"
                                }
                            }
                        }
                    },
                    {
                        "innerError",
                        new OpenApiSchema
                        {
                            UnresolvedReference = true,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = $"{rootNamespaceName}{InnerErrorClassName}"
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for detail property of the error.
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
