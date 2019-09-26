// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Convert settings.
    /// </summary>
    public class OpenApiConvertSettings
    {
        /// <summary>
        /// Gets/sets the service root.
        /// </summary>
        public Uri ServiceRoot { get; set; } = new Uri("http://localhost");

        /// <summary>
        /// Gets/sets the metadata version.
        /// </summary>
        public Version Version { get; set; } = new Version(1, 0, 1);

        /// <summary>
        /// Gets/set a value indicating whether to output key as segment path.
        /// </summary>
        public bool? EnableKeyAsSegment { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output un-qualified operation call.
        /// </summary>
        public bool EnableUnqualifiedCall { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output the path for Edm operation.
        /// </summary>
        public bool EnableOperationPath { get; set; } = true;

        /// <summary>
        /// Gets/set a value indicating whether to output the path for Edm operation import.
        /// </summary>
        public bool EnableOperationImportPath { get; set; } = true;

        /// <summary>
        /// Gets/set a value indicating whether to output the path for Edm navigation property.
        /// </summary>
        public bool EnableNavigationPropertyPath { get; set; } = true;

        /// <summary>
        /// Gets/set a value indicating the tags name depth.
        /// </summary>
        public int TagDepth { get; set; } = 4;

        /// <summary>
        /// Gets/set a value indicating whether we prefix entity type name before single key.
        /// </summary>
        public bool PrefixEntityTypeNameBeforeKey { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether the version of openApi to serialize to is v2.
        /// Currently only impacts nullable references for EdmTypeSchemaGenerator
        /// </summary>
        public OpenApiSpecVersion OpenApiSpecVersion { get; set; } = OpenApiSpecVersion.OpenApi3_0;

        /// <summary>
        /// Gets/sets a value indicating to set the OperationId on Open API operation.
        /// </summary>
        public bool EnableOperationId { get; set; } = true;

        /// <summary>
        /// Gets/sets a value indicating whether to output the binding function as Uri escape function if applied the UriEscapeFunction term.
        /// </summary>
        public bool EnableUriEscapeFunctionCall { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether to verify the edm model before converter.
        /// </summary>
        public bool VerifyEdmModel { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether the server is IEEE754 compatible.
        /// If it is IEEE754Compatible, the server will write quoted string for INT64 and decimal to prevent data loss;
        /// otherwise keep number without quotes.
        /// </summary>
        public bool IEEE754Compatible { get; set; }

        /// <summary>
        /// Gets or sets $Top example value.
        /// </summary>
        public int TopExample { get; set; } = 50;

        internal OpenApiConvertSettings Clone()
        {
            var newSettings = new OpenApiConvertSettings();

            newSettings.ServiceRoot = this.ServiceRoot;
            newSettings.Version = this.Version;
            newSettings.EnableKeyAsSegment = this.EnableKeyAsSegment;
            newSettings.EnableUnqualifiedCall = this.EnableUnqualifiedCall;
            newSettings.EnableOperationPath = this.EnableOperationPath;
            newSettings.EnableOperationImportPath = this.EnableOperationImportPath;
            newSettings.EnableNavigationPropertyPath = this.EnableNavigationPropertyPath;
            newSettings.TagDepth = this.TagDepth;
            newSettings.PrefixEntityTypeNameBeforeKey = this.PrefixEntityTypeNameBeforeKey;
            newSettings.OpenApiSpecVersion = this.OpenApiSpecVersion;
            newSettings.EnableOperationId = this.EnableOperationId;
            newSettings.VerifyEdmModel = this.VerifyEdmModel;
            newSettings.IEEE754Compatible = this.IEEE754Compatible;
            newSettings.TopExample = this.TopExample;
            newSettings.EnableUriEscapeFunctionCall = this.EnableUriEscapeFunctionCall;

            return newSettings;
        }
    }
}
