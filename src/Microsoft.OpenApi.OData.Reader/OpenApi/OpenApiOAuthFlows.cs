//---------------------------------------------------------------------
// <copyright file="OpenApiOAuthFlows.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// OAuth Flows Object.
    /// </summary>
    internal class OpenApiOAuthFlows : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// Configuration for the OAuth Implicit flow
        /// </summary>
        public OpenApiOAuthFlows Implicit { get; set; }

        /// <summary>
        /// Configuration for the OAuth Resource Owner Password flow
        /// </summary>
        public OpenApiOAuthFlows Password { get; set; }

        /// <summary>
        /// Configuration for the OAuth Client Credentials flow.
        /// </summary>
        public OpenApiOAuthFlows ClientCredentials { get; set; }

        /// <summary>
        /// Configuration for the OAuth Authorization Code flow.
        /// </summary>
        public OpenApiOAuthFlows AuthorizationCode { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Any object to the given writer.
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

            // implicit
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocImplicit, Implicit);

            // password
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocPassword, Password);

            // clientCredentials
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocClientCredentials, ClientCredentials);

            // authorizationCode
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocAuthorizationCode, AuthorizationCode);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
