//---------------------------------------------------------------------
// <copyright file="OpenApiServer.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Server Object: an object representing a Server.
    /// </summary>
    internal class OpenApiServer : IOpenApiElement, IOpenApiExtensible, IOpenApiWritable
    {
        /// <summary>
        /// A URL to the target host. This URL supports Server Variables and MAY be relative,
        /// to indicate that the host location is relative to the location
        /// where the OpenAPI document is being served
        /// </summary>
        public Uri Url { get; set; } = OpenApiConstants.OpenApiDocDefaultUrl;

        /// <summary>
        /// An optional string describing the host designated by the URL.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A map between a variable name and its value. 
        /// </summary>
        public IDictionary<string, OpenApiServerVariable> Variables { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Open API server object.
        /// </summary>
        /// <param name="writer">The Open API Writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for JSON, empty for YAML
            writer.WriteStartObject();

            // name
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocUrl, Url.OriginalString);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // variables
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocVariables, Variables);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for JSON, empty for YAML
            writer.WriteEndObject();
        }
    }
}
