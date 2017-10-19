//---------------------------------------------------------------------
// <copyright file="OpenApiSecuritySchema.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// The type of the security scheme.
    /// </summary>
    internal enum SecuritySchemaType
    {
        /// <summary>
        /// apiKey
        /// </summary>
        apiKey,

        /// <summary>
        /// http
        /// </summary>
        http,

        /// <summary>
        /// oauth2
        /// </summary>
        oauth2,

        /// <summary>
        /// openIdConnect
        /// </summary>
        openIdConnect
    }

    internal enum SecuritySchemaLocation
    {
        /// <summary>
        /// query
        /// </summary>
        query,

        /// <summary>
        /// header
        /// </summary>
        header,

        /// <summary>
        /// cookie
        /// </summary>
        cookie
    }

    /// <summary>
    /// Security Scheme Object.
    /// </summary>
    internal class OpenApiSecuritySchema : IOpenApiElement, IOpenApiWritable
    {
        /// <summary>
        /// REQUIRED. The type of the security scheme.
        /// </summary>
        public SecuritySchemaType Type { get; }

        /// <summary>
        /// A short description for security schema.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// REQUIRED. The name of the header, query or cookie parameter to be used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// REQUIRED. The location of the API key.
        /// </summary>
        public SecuritySchemaLocation In { get; set; }

        /// <summary>
        /// REQUIRED. The name of the HTTP Authorization scheme to be used in the Authorization header as defined in RFC7235.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// A hint to the client to identify how the bearer token is formatted.
        /// </summary>
        public string BearerFormat { get; set; }

        /// <summary>
        /// REQUIRED.An object containing configuration information for the flow types supported.
        /// </summary>
        public OpenApiOAuthFlows Flows { get; set; }

        /// <summary>
        /// REQUIRED. OpenId Connect URL to discover OAuth2 configuration values.
        /// </summary>
        public Uri OpenIdConnectUrl { get; set; }

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


            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
