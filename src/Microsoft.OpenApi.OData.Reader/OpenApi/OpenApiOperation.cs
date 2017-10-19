//---------------------------------------------------------------------
// <copyright file="OpenApiOperation.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Operation Object.
    /// </summary>
    internal class OpenApiOperation : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// A list of tags for API documentation control.
        /// </summary>
        public IList<string> Tags { get; set; }

        /// <summary>
        /// A short summary of what the operation does.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// A verbose explanation of the operation behavior.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Additional external documentation for this operation.
        /// </summary>
        public OpenApiExternalDocs ExternalDocs { get; set; }

        /// <summary>
        /// Unique string used to identify the operation.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// A list of parameters.
        /// </summary>
        public IList<OpenApiParameter> Parameters { get; set; }

        /// <summary>
        /// The request body applicable for this operation.
        /// </summary>
        public OpenApiRequestBody RequestBody { get; set; }

        /// <summary>
        /// REQUIRED. The list of possible responses as they are returned from executing this operation.
        /// </summary>
        public OpenApiResponses Responses { get; set; }

        /// <summary>
        /// A map of possible out-of band callbacks related to the parent operation.
        /// </summary>
        public IDictionary<string, OpenApiCallback> Callbacks { get; set; }

        /// <summary>
        /// Declares this operation to be deprecated.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Security requirement list.
        /// </summary>
        public IList<OpenApiSecurityRequirement> Security { get; set; }

        /// <summary>
        /// An alternative server array to service this operation.
        /// </summary>
        public IList<OpenApiServer> Servers { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Open API operation object.
        /// </summary>
        /// <param name="writer">The Open API Writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            // summary
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocSummary, Summary);

            // tags
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocTags, Tags);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // externalDocs
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocExternalDocs, ExternalDocs);

            // operationId
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocOperationId, OperationId);

            // parameters
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocParameters, Parameters);

            // requestBody
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocRequestBody, RequestBody);

            // responses
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocResponses, Responses);

            // callbacks
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocCallbacks, Callbacks);

            // deprecated
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDeprecated, Deprecated);

            // security
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocSecurity, Security);

            // servers
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocServers, Servers);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
