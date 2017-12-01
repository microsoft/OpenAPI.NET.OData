// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

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
                { "default",
                    new OpenApiResponse
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Response,
                            Id = "error"
                        }
                    }
                },

                { "204", new OpenApiResponse { Description = "Success"} },
           };

        /// <summary>
        /// Get the <see cref="OpenApiResponse"/> for the build-in statusCode.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>The created <see cref="OpenApiResponse"/>.</returns>
        public static OpenApiResponse GetResponse(this string statusCode)
        {
            OpenApiResponse response;
            if (_responses.TryGetValue(statusCode, out response))
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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            return new Dictionary<string, OpenApiResponse>
            {
                { "error", CreateErrorResponse() }
            };
        }

        /// <summary>
        /// Create the <see cref="OpenApiResponses"/> for a <see cref="IEdmOperationImport"/>
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>The created <see cref="OpenApiResponses"/>.</returns>
        public static OpenApiResponses CreateResponses(this ODataContext context, IEdmOperationImport operationImport)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (operationImport == null)
            {
                throw Error.ArgumentNull(nameof(operationImport));
            }

            return context.CreateResponses(operationImport.Operation);
        }

        /// <summary>
        /// Create the <see cref="OpenApiResponses"/> for a <see cref="IEdmOperation"/>
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operation">The Edm operation.</param>
        /// <returns>The created <see cref="OpenApiResponses"/>.</returns>
        public static OpenApiResponses CreateResponses(this ODataContext context, IEdmOperation operation)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (operation == null)
            {
                throw Error.ArgumentNull(nameof(operation));
            }

            OpenApiResponses responses = new OpenApiResponses();

            if (operation.IsAction())
            {
                responses.Add("204", "204".GetResponse());
            }
            else
            {
                OpenApiResponse response = new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json",
                            new OpenApiMediaType
                            {
                                Schema = context.CreateEdmTypeSchema(operation.ReturnType)
                            }
                        }
                    }
                };
                responses.Add("200", response);
            }

            // both action & function has the default response.
            responses.Add("default", "default".GetResponse());

            return responses;
        }

        private static OpenApiResponse CreateErrorResponse()
        {
            return new OpenApiResponse
            {
                Description = "error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
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
