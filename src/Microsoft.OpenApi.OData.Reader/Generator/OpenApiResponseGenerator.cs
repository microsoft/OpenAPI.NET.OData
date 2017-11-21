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
    /// Class to create <see cref="OpenApiResponse"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal class OpenApiResponseGenerator : OpenApiGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiResponseGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api convert settings.</param>
        public OpenApiResponseGenerator(IEdmModel model, OpenApiConvertSettings settings)
            : base(model, settings)
        {
        }

        /// <summary>
        /// The value of responses is a map of Response Objects.
        /// It contains one name/value pair for the standard OData error response
        /// that is referenced from all operations of the service.
        /// </summary>
        /// <returns>The name/value pairs for the standard OData error response.</returns>
        public IDictionary<string, OpenApiResponse> CreateResponses()
        {
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
