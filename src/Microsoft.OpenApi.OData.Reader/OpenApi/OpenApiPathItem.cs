//---------------------------------------------------------------------
// <copyright file="OpenApiPathItem.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Path Item Object: to describe the operations available on a single path.
    /// </summary>
    internal class OpenApiPathItem : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible, IOpenApiReferencable
    {
        /// <summary>
        /// An optional, string summary.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// An optional, string description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A definition of a GET operation on this path.
        /// </summary>
        public OpenApiOperation Get { get; set; }

        /// <summary>
        /// A definition of a PUT operation on this path.
        /// </summary>
        public OpenApiOperation Put { get; set; }

        /// <summary>
        /// A definition of a POST operation on this path.
        /// </summary>
        public OpenApiOperation Post { get; set; }

        /// <summary>
        /// A definition of a DELETE operation on this path.
        /// </summary>
        public OpenApiOperation Delete { get; set; }

        /// <summary>
        /// A definition of a OPTIONS operation on this path.
        /// </summary>
        public OpenApiOperation Options { get; set; }

        /// <summary>
        /// A definition of a HEAD operation on this path.
        /// </summary>
        public OpenApiOperation Head { get; set; }

        /// <summary>
        /// A definition of a PATCH  operation on this path.
        /// </summary>
        public OpenApiOperation Patch { get; set; }

        /// <summary>
        /// A definition of a TRACE   operation on this path.
        /// </summary>
        public OpenApiOperation Trace { get; set; }

        /// <summary>
        /// An alternative server array to service all operations in this path.
        /// </summary>
        public IList<OpenApiServer> Servers { get; set; }

        /// <summary>
        /// A list of parameters 
        /// </summary>
        public IList<OpenApiParameter> Parameters { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Reference Object.
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
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            // summary
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocSummary, Summary);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // get
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocGet, Get);

            // put
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocPut, Put);

            // post
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocPost, Post);

            // delete
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocDelete, Delete);

            // options
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocOptions, Options);

            // head
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocHead, Head);

            // patch
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocPatch, Patch);

            // trace
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocTrace, Trace);

            // servers
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocServers, Servers);

            // parameters
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocParameters, Parameters);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
