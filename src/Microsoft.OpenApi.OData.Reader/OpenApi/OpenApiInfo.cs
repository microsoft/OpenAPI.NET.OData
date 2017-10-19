//---------------------------------------------------------------------
// <copyright file="OpenApiInfo.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Open API Info Object, it provides the metadata about the Open API.
    /// </summary>
    internal class OpenApiInfo : IOpenApiElement, IOpenApiWritable, IOpenApiExtensible
    {
        /// <summary>
        /// REQUIRED. The title of the application.
        /// </summary>
        public string Title { get; set; } = OpenApiConstants.OpenApiDocDefaultTitle;

        /// <summary>
        /// REQUIRED. The version of the OpenAPI document.
        /// </summary>
        public Version Version { get; set; } = OpenApiConstants.OpenApiDocDefaultVersion;

        /// <summary>
        /// A short description of the application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A URL to the Terms of Service for the API. MUST be in the format of a URL.
        /// </summary>
        public Uri TermsOfService { get; set; }

        /// <summary>
        /// The contact information for the exposed API.
        /// </summary>
        public OpenApiContact Contact { get; set; }

        /// <summary>
        /// The license information for the exposed API.
        /// </summary>
        public OpenApiLicense License { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write Open API Info to the given writer.
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

            // title
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocTitle, Title);

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // termsOfService
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocTermsOfService, TermsOfService?.OriginalString);

            // contact object
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocContact, Contact);

            // license object
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocLicense, License);

            // version
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocVersion, Version?.ToString());

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
