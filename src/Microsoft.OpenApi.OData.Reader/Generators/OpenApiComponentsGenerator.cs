// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generators
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiComponents"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiComponentsGenerator
    {
        /// <summary>
        /// Generate the <see cref="OpenApiComponents"/>.
        /// The value of components is a Components Object.
        /// It holds maps of reusable schemas describing message bodies, operation parameters, and responses.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The components object.</returns>
        public static OpenApiComponents CreateComponents(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
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
                Schemas = model.CreateSchemas(),

                // The value of parameters is a map of Parameter Objects.
                // It allows defining query options and headers that can be reused across operations of the service.
                Parameters = model.CreateParameters(),

                // The value of responses is a map of Response Objects.
                // It allows defining responses that can be reused across operations of the service.
                Responses = model.CreateResponses()
            };
        }
    }
}
