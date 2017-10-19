//---------------------------------------------------------------------
// <copyright file="OpenApiLink.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Link Object.
    /// </summary>
    internal class OpenApiLink : IOpenApiElement, IOpenApiExtensible, IOpenApiWritable, IOpenApiReferencable
    {
        private string _operationRef;
        private string _operationId;
        private string _description;
        private IDictionary<string, OpenApiAny> _parameters;
        private OpenApiAny _requestBody;
        private OpenApiServer _server;
        private IList<OpenApiExtension> _extensions;

        /// <summary>
        /// A relative or absolute reference to an OAS operation.
        /// </summary>
        public string OperationRef
        {
            get
            {
                return _operationRef;
            }
            set
            {
                this.ValidateReference();

                _operationRef = value;

                // This field is mutually exclusive of the operationId field
                _operationId = null;
            }
        }

        /// <summary>
        /// The name of an existing, resolvable OAS operation, as defined with a unique operationId.
        /// </summary>
        public string OperationId
        {
            get
            {
                return _operationId;
            }
            set
            {
                this.ValidateReference();

                _operationId = value;

                ///This field is mutually exclusive of the operationRef field.
                _operationRef = null;
            }
        }

        /// <summary>
        /// A map representing parameters to pass to an operation
        /// as specified with operationId or identified via operationRef.
        /// </summary>
        public IDictionary<string, OpenApiAny> Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                this.ValidateReference();
                _parameters = value;
            }
        }

        /// <summary>
        /// A literal value or {expression} to use as a request body when calling the target operation.
        /// </summary>
        public OpenApiAny RequestBody
        {
            get
            {
                return _requestBody;
            }
            set
            {
                this.ValidateReference();
                _requestBody = value;
            }
        }

        /// <summary>
        /// A description of the link.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                this.ValidateReference();
                _description = value;
            }
        }

        /// <summary>
        /// A server object to be used by the target operation.
        /// </summary>
        public OpenApiServer Server
        {
            get { return _server; }
            set
            {
                this.ValidateReference();
                _server = value;
            }
        }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions
        {
            get
            {
                return _extensions;
            }
            set
            {
                this.ValidateReference();
                _extensions = value;
            }
        }

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
            // { for JSON, empty for YAML
            writer.WriteStartObject();

            // operationRef
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocOperationRef, OperationRef);

            // operationId
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocOperationId, OperationId);

            // parameters
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocParameters, Parameters);

            // requestBody
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocRequestBody, RequestBody);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // server
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocServer, Server);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for JSON, empty for YAML
            writer.WriteEndObject();
        }
    }
}
