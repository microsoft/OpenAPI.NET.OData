// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiComponents"/> by Edm model.
    /// </summary>
    internal static class OpenApiComponentsGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiComponents"/>.
        /// The value of components is a Components Object.
        /// It holds maps of reusable schemas describing message bodies, operation parameters, and responses.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The created <see cref="OpenApiComponents"/> object.</returns>
        public static OpenApiComponents CreateComponents(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            // "components": {
            //   "schemas": …,
            //   "parameters": …,
            //   "responses": …,
            //   "requestBodies": … 
            //  }
            return new OpenApiComponents
            {
                // The value of schemas is a map of Schema Objects.
                // Each entity type, complex type, enumeration type, and type definition directly
                // or indirectly used in the paths field is represented as a name/value pair of the schemas map.
                Schemas = context.CreateSchemas(),

                // The value of parameters is a map of Parameter Objects.
                // It allows defining query options and headers that can be reused across operations of the service.
                Parameters = context.CreateParameters(),

                // The value of responses is a map of Response Objects.
                // It allows defining responses that can be reused across operations of the service.
                Responses = context.CreateResponses(),

                // The value of requestBodies is a map of RequestBody Objects.
                // It allows refining request bodies that can be reused across operations of the service.
                RequestBodies = context.CreateRequestBodies(),

                Examples = context.CreateExamples(),

                SecuritySchemes = context.CreateSecuritySchemes(),

                // Make others as null.
                Links = null,

                Callbacks = null,

                Extensions = null
            };
        }
    }
}
