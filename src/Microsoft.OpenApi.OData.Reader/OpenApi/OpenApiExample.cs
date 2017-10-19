//---------------------------------------------------------------------
// <copyright file="OpenApiExample.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Example Object.
    /// </summary>
    internal class OpenApiExample : IOpenApiElement, IOpenApiExtensible, IOpenApiWritable, IOpenApiReferencable
    {
        private OpenApiAny _value;
        private Uri _externalValue;

        /// <summary>
        /// Short description for the example.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Long description for the example.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Embedded literal example.
        /// </summary>
        public OpenApiAny Value
        {
            get
            {
                return _value;
            }
            set
            {
                // The value field and external Value field are mutually exclusive.
                _externalValue = null;
                _value = value;
            }
        }

        /// <summary>
        /// A URL that points to the literal example. 
        /// </summary>
        public Uri ExternalValue
        {
            get
            {
                return _externalValue;
            }
            set
            {
                // The value field and external Value field are mutually exclusive.
                _value = null;
                _externalValue = value;
            }
        }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Reference Object.
        /// </summary>
        public OpenApiReference Reference { get; set; }

        /// <summary>
        /// Write Example object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void Write(IOpenApiWriter writer)
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
            // { for json, empty for YAML
            writer.WriteStartObject();

            // summary
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocSummary, Summary);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // value
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocValue, Value);

            // externalValue
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocExternalValue, ExternalValue?.OriginalString);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
