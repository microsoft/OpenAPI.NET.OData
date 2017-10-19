//---------------------------------------------------------------------
// <copyright file="OpenApiServerVariable.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Server Variable Object.
    /// </summary>
    internal class OpenApiServerVariable : IOpenApiElement, IOpenApiExtensible, IOpenApiWritable
    {
        /// <summary>
        /// REQUIRED. The default value to use for substitution, and to send,
        /// if an alternate value is not supplied.
        /// </summary>
        public string Default { get; set; } = OpenApiConstants.OpenApiDocDefaultDefault;

        /// <summary>
        /// An optional description for the server variable.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An enumeration of string values to be used if the substitution options are from a limited set.
        /// </summary>
        public IList<string> Enums { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Open API server variable object.
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

            // default
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocDefault, Default);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // enums
            if (Enums != null && Enums.Any())
            {
                writer.WritePropertyName(OpenApiConstants.OpenApiDocEnum);
                writer.WriteStartArray();
                foreach(string item in Enums)
                {
                    writer.WriteValue(item);
                }
                writer.WriteEndArray();
            }

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for JSON, empty for YAML
            writer.WriteEndObject();
        }
    }
}
