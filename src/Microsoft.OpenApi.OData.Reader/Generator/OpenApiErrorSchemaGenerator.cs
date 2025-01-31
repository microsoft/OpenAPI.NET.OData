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
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Models.Interfaces;

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
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The string/schema dictionary.</returns>
        public static IDictionary<string, IOpenApiSchema> CreateODataErrorSchemas(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            var rootNamespaceName = context.GetErrorNamespaceName();

            return new Dictionary<string, IOpenApiSchema>()
            {
                { $"{rootNamespaceName}{ODataErrorClassName}", CreateErrorSchema(rootNamespaceName, document) },
                { $"{rootNamespaceName}{MainErrorClassName}", CreateErrorMainSchema(rootNamespaceName, document) },
                { $"{rootNamespaceName}{ErrorDetailsClassName}", CreateErrorDetailSchema() },
                { $"{rootNamespaceName}{InnerErrorClassName}", CreateInnerErrorSchema(context, document) }
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
        /// <param name="document">The Open API document to lookup references.</param>
        public static OpenApiSchema CreateErrorSchema(string rootNamespaceName, OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Required = new HashSet<string>
                {
                    "error"
                },
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    {
                        "error",
                        new OpenApiSchemaReference($"{rootNamespaceName}{MainErrorClassName}", document)
                    }
                }
            };
        }

        /// <summary>
        /// Creates the inner error schema definition. If an "InnerError" complex type is defined in the root namespace, then this type will be used as the inner error type.
        /// Otherwise, a default inner error type of object will be created.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The inner error schema definition.</returns>
        public static IOpenApiSchema CreateInnerErrorSchema(ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            var rootNamespace = context.Model.DeclaredNamespaces.OrderBy(n => n.Count(x => x == '.')).FirstOrDefault();
            if(!string.IsNullOrEmpty(context.Settings.InnerErrorComplexTypeName) &&
                !string.IsNullOrEmpty(rootNamespace) &&
                context.Model.FindDeclaredType($"{rootNamespace}.{context.Settings.InnerErrorComplexTypeName}") is IEdmComplexType complexType)
            {
                return context.CreateSchemaTypeSchema(complexType, document);
            }
            
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "The structure of this object is service-specific"
            };
        }

        /// <summary>
        /// Create <see cref="OpenApiSchema"/> for main property of the error.
        /// </summary>
        /// <param name="rootNamespaceName">The root namespace name. With a trailing dot.</param>
        /// <param name="document">The Open API document to lookup references.</param>
        /// <returns>The created <see cref="OpenApiSchema"/>.</returns>
        public static OpenApiSchema CreateErrorMainSchema(string rootNamespaceName, OpenApiDocument document)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Required = new HashSet<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = JsonSchemaType.String, Nullable = false }
                    },
                    {
                        "message", new OpenApiSchema { Type = JsonSchemaType.String, Nullable = false, Extensions = new Dictionary<string, IOpenApiExtension> 
                                                                                    { { OpenApiPrimaryErrorMessageExtension.Name, new OpenApiPrimaryErrorMessageExtension { IsPrimaryErrorMessage = true } } } }
                    },
                    {
                        "target", new OpenApiSchema { Type = JsonSchemaType.String, Nullable = true }
                    },
                    {
                        "details",
                        new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchemaReference($"{rootNamespaceName}{ErrorDetailsClassName}", document)
                        }
                    },
                    {
                        "innerError",
                        new OpenApiSchemaReference($"{rootNamespaceName}{InnerErrorClassName}", document)
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
                Type = JsonSchemaType.Object,
                Required = new HashSet<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = JsonSchemaType.String, Nullable = false, }
                    },
                    {
                        "message", new OpenApiSchema { Type = JsonSchemaType.String, Nullable = false, }
                    },
                    {
                        "target", new OpenApiSchema { Type = JsonSchemaType.String, Nullable = true, }
                    }
                }
            };
        }
    }
}
