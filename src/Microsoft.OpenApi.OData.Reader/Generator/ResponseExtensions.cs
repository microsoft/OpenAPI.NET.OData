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
    /// Extension methods to create <see cref="OpenApiResponse"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class ResponseExtension
    {
        private static IDictionary<string, OpenApiResponse> Responses =
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

        public static OpenApiResponse GetResponse(this string statusCode)
        {
            return Responses[statusCode];
        }

        public static OpenApiResponses CreateResponses(this IEdmAction actionImport)
        {
            return new OpenApiResponses
            {
                { "204", "204".GetResponse() },
                { "default", "default".GetResponse() }
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
            responses.Add("default", "default".GetResponse());
            return responses;
        }
    }
}
