//---------------------------------------------------------------------
// <copyright file="OpenApiLicense.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// License Object.
    /// </summary>
    internal class OpenApiLicense : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// REQUIRED.The license name used for the API.
        /// </summary>
        public string Name { get; set; } = OpenApiConstants.OpenApiDocDefaultName;

        /// <summary>
        /// The URL pointing to the contact information. MUST be in the format of a URL.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Open API license object to the given writer.
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

            // name
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocName, Name);

            // url
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocUrl, Url?.OriginalString);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
