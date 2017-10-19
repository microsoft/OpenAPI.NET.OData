//---------------------------------------------------------------------
// <copyright file="OpenApiEncoding.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Encoding Object.
    /// </summary>
    internal class OpenApiEncoding : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// The Content-Type for encoding a specific property.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// A map allowing additional information to be provided as headers.
        /// </summary>
        public IDictionary<string, OpenApiHeader> Headers { get; set; }

        /// <summary>
        /// Describes how a specific property value will be serialized depending on its type. 
        /// </summary>
        public ParameterStyle? Style { get; set; }

        /// <summary>
        /// Explode
        /// </summary>
        public bool? Explode { get; set; }

        /// <summary>
        /// AllowReserved
        /// </summary>
        public bool? AllowReserved { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Encoding object to the given writer.
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

            // contentType
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocContentType, ContentType);

            // headers
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocHeaders, Headers);

            // style
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocStyle, Style?.ToString());

            // explode
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocExplode, Explode);

            // allowReserved
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocAllowReserved, AllowReserved);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
