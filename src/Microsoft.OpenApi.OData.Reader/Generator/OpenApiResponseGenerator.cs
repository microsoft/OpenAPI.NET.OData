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
    internal static class OpenApiResponseGenerator
    {
        /// <summary>
        /// 4.6.3 Field responses in components
        /// The value of responses is a map of Response Objects.
        /// It contains one name/value pair for the standard OData error response
        /// that is referenced from all operations of the service.
        /// </summary>
        /// <returns>The name/value pairs for the standard OData error response.</returns>
        public static IDictionary<string, OpenApiResponse> CreateResponses(this IEdmModel model,
            OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                throw Error.ArgumentNull(nameof(settings));
            }

            return new Dictionary<string, OpenApiResponse>
            {
                { "error", CreateErrorResponse() }
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
