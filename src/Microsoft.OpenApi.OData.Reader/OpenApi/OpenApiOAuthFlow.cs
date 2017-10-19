//---------------------------------------------------------------------
// <copyright file="OpenApiOAuthFlow.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// OAuth Flow Object.
    /// </summary>
    internal class OpenApiOAuthFlow : IOpenApiElement, IOpenApiWritable,IOpenApiExtensible
    {
        /// <summary>
        /// REQUIRED. The authorization URL to be used for this flow. 
        /// </summary>
        public Uri AuthorizationUrl { get; set; }

        /// <summary>
        /// REQUIRED. The token URL to be used for this flow.
        /// </summary>
        public Uri TokenUrl { get; set; }

        /// <summary>
        /// The URL to be used for obtaining refresh tokens.
        /// </summary>
        public Uri RefreshUrl { get; set; }

        /// <summary>
        /// REQUIRED. The available scopes for the OAuth2 security scheme.
        /// A map between the scope name and a short description for it.
        /// </summary>
        public IDictionary<string, string> Scopes { get; set; }

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

            // authorizationUrl
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocAuthorizationUrl, AuthorizationUrl?.OriginalString);

            // tokenUrl
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocTokenUrl, TokenUrl?.OriginalString);

            // refreshUrl
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocRefreshUrl, RefreshUrl?.OriginalString);

            // scopes
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocScopes, Scopes);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
