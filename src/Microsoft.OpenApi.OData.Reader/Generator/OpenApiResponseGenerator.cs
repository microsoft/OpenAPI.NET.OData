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
    /// Extension methods to create <see cref="OpenApiResponse"/> by Edm model.
    /// </summary>
    internal static class OpenApiResponseGenerator
    {
        private static IDictionary<string, OpenApiResponse> _responses =
           new Dictionary<string, OpenApiResponse>
           {
                { Constants.StatusCodeDefault,
                    new OpenApiResponse
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Response,
                            Id = "error"
                        }
                    }
                },

                { Constants.StatusCode204, new OpenApiResponse { Description = "Success"} },
                { Constants.StatusCodeClass4XX, new OpenApiResponse
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Response,
                            Id = "error"
                        }
                    }
                },
                { Constants.StatusCodeClass5XX, new OpenApiResponse
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Response,
                            Id = "error"
                        }
                    }
                }
           };

        /// <summary>
        /// Get the <see cref="OpenApiResponse"/> for the build-in statusCode.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>The created <see cref="OpenApiResponse"/>.</returns>
        public static OpenApiResponse GetResponse(this string statusCode)
        {
            if (_responses.TryGetValue(statusCode, out OpenApiResponse response))
            {
                return response;
            }

            return null;
        }

        /// <summary>
        /// Field responses in components
        /// The value of responses is a map of Response Objects.
        /// It contains one name/value pair for the standard OData error response
        /// that is referenced from all operations of the service.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <returns>The name/value pairs for the standard OData error response.</returns>
        public static IDictionary<string, OpenApiResponse> CreateResponses(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            var responses =  new Dictionary<string, OpenApiResponse>
            {
                { "error", CreateErrorResponse() }
            };

            if(context.Settings.EnableDollarCountPath)
            {
                responses[Constants.DollarCountSchemaName] = CreateCountResponse();
            }

            responses = responses.Concat(context.GetAllCollectionEntityTypes()
                                        .Select(x => new KeyValuePair<string, OpenApiResponse>(
                                                            $"{(x is IEdmEntityType eType ? eType.FullName() : x.FullTypeName())}{Constants.CollectionSchemaSuffix}",
                                                            CreateCollectionResponse(x)))
                                        .Where(x => !responses.ContainsKey(x.Key)))
                                .Concat(context.GetAllCollectionComplexTypes()
                                        .Select(x => new KeyValuePair<string, OpenApiResponse>(
                                                            $"{x.FullTypeName()}{Constants.CollectionSchemaSuffix}",
                                                            CreateCollectionResponse(x)))
                                        .Where(x => !responses.ContainsKey(x.Key)))
                            .ToDictionary(x => x.Key, x => x.Value);

            if(context.HasAnyNonContainedCollections())                                        
                responses[$"String{Constants.CollectionSchemaSuffix}"] = CreateCollectionResponse("String");

            return responses;
        }

        /// <summary>
        /// Create the <see cref="OpenApiResponses"/> for a <see cref="IEdmOperationImport"/>
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <param name="path">The OData path.</param>
        /// <returns>The created <see cref="OpenApiResponses"/>.</returns>
        public static OpenApiResponses CreateResponses(this ODataContext context, IEdmOperationImport operationImport, ODataPath path)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(operationImport, nameof(operationImport));
            Utils.CheckArgumentNull(path, nameof(path));

            return context.CreateResponses(operationImport.Operation, path);
        }

        /// <summary>
        /// Create the <see cref="OpenApiResponses"/> for a <see cref="IEdmOperation"/>
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operation">The Edm operation.</param>
        /// <param name="path">The OData path.</param>
        /// <returns>The created <see cref="OpenApiResponses"/>.</returns>
        public static OpenApiResponses CreateResponses(this ODataContext context, IEdmOperation operation, ODataPath path)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(operation, nameof(operation));
            Utils.CheckArgumentNull(path, nameof(path));

            OpenApiResponses responses = new();

            if (operation.IsAction() && operation.ReturnType == null)
            {
                responses.Add(Constants.StatusCode204, Constants.StatusCode204.GetResponse());
            }
            else
            {
                OpenApiSchema schema;
                if (operation.ReturnType.IsCollection())
                {
                    // Get the entity type of the previous segment
                    IEdmEntityType entityType = path.Segments.Reverse().Skip(1)?.Take(1)?.FirstOrDefault()?.EntityType;
                    schema = new OpenApiSchema
                    {
                        Title = entityType == null ? null : $"Collection of {entityType.Name}",
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            {
                                "value", context.CreateEdmTypeSchema(operation.ReturnType)
                            }
                        }
                    };
                }
                else if (operation.ReturnType.IsPrimitive())
                {
                    // A property or operation response that is of a primitive type is represented as an object with a single name/value pair,
                    // whose name is value and whose value is a primitive value.
                    schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            {
                                "value", context.CreateEdmTypeSchema(operation.ReturnType)
                            }
                        }
                    };
                }
                else
                {
                    schema = context.CreateEdmTypeSchema(operation.ReturnType);
                }

                OpenApiResponse response = new()
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            Constants.ApplicationJsonMediaType,
                            new OpenApiMediaType
                            {
                                Schema = schema
                            }
                        }
                    }
                };
                responses.Add(Constants.StatusCode200, response);
            }

            // Both action & function have the default response.
            responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

            return responses;
        }

        private static OpenApiResponse CreateCollectionResponse(IEdmStructuredType structuredType)
        {
            var entityType = structuredType as IEdmEntityType;
            return CreateCollectionResponse(entityType?.FullName() ?? structuredType.FullTypeName());
        }
        private static OpenApiResponse CreateCollectionResponse(string typeName)
        {
            return new OpenApiResponse
            {
                Description = "Retrieved collection",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType,
                        new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = $"{typeName}{Constants.CollectionSchemaSuffix}"
                                }
                            }
                        }
                    }
                }
            };
        }

        private static OpenApiResponse CreateCountResponse()
        {
            OpenApiSchema schema = new()
            {
                Reference = new() {
                    Type = ReferenceType.Schema,
                    Id = Constants.DollarCountSchemaName
                }
            };
            return new OpenApiResponse
            {
                Description = "The count of the resource",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "text/plain",
                        new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    }
                }
            };
        }

        private static OpenApiResponse CreateErrorResponse()
        {
            return new OpenApiResponse
            {
                Description = "error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        Constants.ApplicationJsonMediaType,
                        new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "odata.error"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
