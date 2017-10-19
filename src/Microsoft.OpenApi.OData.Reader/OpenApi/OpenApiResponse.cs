//---------------------------------------------------------------------
// <copyright file="OpenApiResponse.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.OpenAPI.Properties;

namespace Microsoft.OData.OpenAPI
{
    internal class OpenApiResponse : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible, IOpenApiReferencable
    {
        private IDictionary<string, OpenApiHeader> _headers;
        private IDictionary<string, OpenApiMediaType> _content;
        private IDictionary<string, OpenApiLink> _link;

        /// <summary>
        /// REQUIRED. A short description of the response.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Maps a header name to its definition.
        /// </summary>
        public IDictionary<string, OpenApiHeader> Headers { get; set; }

        /// <summary>
        /// A map containing descriptions of potential response payloads.
        /// </summary>
        public IDictionary<string, OpenApiMediaType> Content { get; set; }

        /// <summary>
        /// A map of operations links that can be followed from the response.
        /// </summary>
        public IDictionary<string, OpenApiLink> Links { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Reference object.
        /// </summary>
        public OpenApiReference Reference { get; set; }

        /// <summary>
        /// Write Open API response to given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (Reference != null)
            {
                Reference.Write(writer);
            }
            else
            {
                WriteInternal(writer);
            }
        }

        private void WriteInternal(IOpenApiWriter writer)
        {
            Debug.Assert(writer != null);

            // { for json, empty for YAML
            writer.WriteStartObject();

            // description
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // headers
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocHeaders, Headers);

            // content
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocContent, Content);

            // headers
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocLinks, Links);

            // Extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
