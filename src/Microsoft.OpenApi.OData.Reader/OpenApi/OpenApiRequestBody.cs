//---------------------------------------------------------------------
// <copyright file="OpenApiRequestBody.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Request Body Object
    /// </summary>
    internal class OpenApiRequestBody : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// A brief description of the request body.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// REQUIRED. The content of the request body. 
        /// </summary>
        public IDictionary<string, OpenApiMediaType> Content;

        /// <summary>
        /// Determines if the request body is required in the request. Defaults to false.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Any object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // content
            writer.WriteRequiredDictionary(OpenApiConstants.OpenApiDocContent, Content);

            // required
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocRequired, Required);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
