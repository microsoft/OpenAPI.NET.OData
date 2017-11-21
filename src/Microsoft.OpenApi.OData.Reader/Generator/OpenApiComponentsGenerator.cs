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
    /// Class to create <see cref="OpenApiComponents"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiComponentsGenerator
    {
        /// <summary>
        /// Generate the <see cref="OpenApiComponents"/>.
        /// The value of components is a Components Object.
        /// It holds maps of reusable schemas describing message bodies, operation parameters, and responses.
        /// </summary>
        /// <returns>The components object.</returns>
        public static OpenApiComponents CreateComponents(this IEdmModel model, OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                throw Error.ArgumentNull(nameof(settings));
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
                Schemas = CreateSchemas(),

                // The value of parameters is a map of Parameter Objects.
                // It allows defining query options and headers that can be reused across operations of the service.
                Parameters = CreateParameters(),

                // The value of responses is a map of Response Objects.
                // It allows defining responses that can be reused across operations of the service.
                Responses = CreateResponses()
            };
        }

        /// <summary>
        ///  Field Schemas
        /// </summary>
        private IDictionary<string, OpenApiSchema> CreateSchemas()
        {
            OpenApiSchemaGenerator schemaGenerator = new OpenApiSchemaGenerator(Model, Settings);
            return schemaGenerator.CreateSchemas();
        }

        /// <summary>
        /// Field parameters
        /// </summary>
        private IDictionary<string, OpenApiParameter> CreateParameters()
        {
            OpenApiParameterGenerator parameterGenerator = new OpenApiParameterGenerator(Model, Settings);
            return parameterGenerator.CreateParameters();
        }

        /// <summary>
        /// Field responses
        /// </summary>
        private IDictionary<string, OpenApiResponse> CreateResponses()
        {
            OpenApiResponseGenerator responseGenerator = new OpenApiResponseGenerator(Model, Settings);
            return responseGenerator.CreateResponses();
        }
    }
}
