//---------------------------------------------------------------------
// <copyright file="OpenApiComponents.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Components Object.
    /// </summary>
    internal class OpenApiComponents : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// Schemas
        /// </summary>
        public IDictionary<string, OpenApiSchema> Schemas { get; set; }

        /// <summary>
        /// Responses
        /// </summary>
        public IDictionary<string, OpenApiResponse> Responses { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public IDictionary<string, OpenApiParameter> Parameters { get; set; }

        /// <summary>
        /// Examples
        /// </summary>
        public IDictionary<string, OpenApiExample> Examples { get; set; }

        /// <summary>
        /// RequestBodies
        /// </summary>
        public IDictionary<string, OpenApiRequestBody> RequestBodies { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public IDictionary<string, OpenApiHeader> Headers { get; set; }

        /// <summary>
        /// SecuritySchemes
        /// </summary>
        public IDictionary<string, OpenApiSecuritySchema> SecuritySchemes { get; set; }

        /// <summary>
        /// Links
        /// </summary>
        public IDictionary<string, OpenApiLink> Links { get; set; }

        /// <summary>
        /// Callbacks
        /// </summary>
        public IDictionary<string, OpenApiCallback> Callbacks { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write components object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            // schemas
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocSchemas, Schemas);

            // responses
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocResponses, Responses);

            // parameters
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocParameters, Parameters);

            // examples
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocExamples, Examples);

            // requestBodies
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocRequestBodies, RequestBodies);

            // headers
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocHeaders, Headers);

            // securitySchemes
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocSecuritySchemes, SecuritySchemes);

            // links
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocLinks, Links);

            // callbacks
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocCallbacks, Callbacks);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
