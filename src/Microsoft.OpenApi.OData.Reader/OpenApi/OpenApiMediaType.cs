//---------------------------------------------------------------------
// <copyright file="OpenApiMediaType.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Media Type Object.
    /// </summary>
    internal class OpenApiMediaType : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// The schema defining the type used for the request body.
        /// </summary>
        public OpenApiSchema Schema { get; set; }

        /// <summary>
        /// Example of the media type. 
        /// </summary>
        public OpenApiAny Example { get; set; }

        /// <summary>
        /// Examples of the media type.
        /// </summary>
        public IDictionary<string, OpenApiExample> Examples { get; set; }

        /// <summary>
        /// A map between a property name and its encoding information. 
        /// </summary>
        public IDictionary<string, OpenApiEncoding> Encoding { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write media type object to the given writer.
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

            // schema
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocSchema, Schema);

            // example
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocExample, Example);

            // examples
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocExamples, Examples);

            // encoding
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocEncoding, Encoding);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
