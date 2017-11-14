// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiResponse"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiResponseGenerator
    {
        private static IDictionary<string, OpenApiResponse> Responses =
           new Dictionary<string, OpenApiResponse>
           {
                { "default",
                    new OpenApiResponse
                    {
                        Pointer = new OpenApiReference(ReferenceType.Response, "error")
                    }
                },
                { "204", new OpenApiResponse { Description = "Success"} },
           };

        public static KeyValuePair<string, OpenApiResponse> GetResponse(this string statusCode)
        {
            return new KeyValuePair<string, OpenApiResponse>(statusCode, Responses[statusCode]);
        }

        /// <summary>
        /// Create <see cref="OpenApiInfo"/> object.
        /// It contains one name/value pair for the standard OData error response
        /// that is referenced from all operations of the service.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The name/value pairs for the standard OData error response.</returns>
        public static IDictionary<string, OpenApiResponse> CreateResponses(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            return new Dictionary<string, OpenApiResponse>
            {
                { "error", CreateError() }
            };
        }

        public static OpenApiResponses CreateResponses(this IEdmAction actionImport)
        {
            return new OpenApiResponses
            {
                "204".GetResponse(),
                "default".GetResponse()
            };
        }

        public static OpenApiResponses CreateResponses(this IEdmFunction function)
        {
            OpenApiResponses responses = new OpenApiResponses();

            OpenApiResponse response = new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
                        new OpenApiMediaType
                        {
                            Schema = function.ReturnType.CreateSchema()
                        }
                    }
                }
            };
            responses.Add("200", response);
            responses.Add("default".GetResponse());
            return responses;
        }

        private static OpenApiResponse CreateError()
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
                                Pointer = new OpenApiReference(ReferenceType.Schema, "odata.error")
                            }
                        }
                    }
                }
            };
        }
    }
}
