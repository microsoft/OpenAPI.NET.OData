// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;

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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // "components": {
            //   "schemas": …,
            //   "parameters": …,
            //   "responses": …
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

                // Make others as null.
                RequestBodies = null,

                Examples = null,

                SecuritySchemes = null,

                Links = null,

                Callbacks = null,

                Extensions = null
            };
        }
    }
}
