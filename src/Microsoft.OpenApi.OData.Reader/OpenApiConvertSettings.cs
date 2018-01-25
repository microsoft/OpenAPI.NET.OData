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
        public bool KeyAsSegment { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output un-qualified operation call.
        /// </summary>
        public bool UnqualifiedCall { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output case-insensitive path.
        /// </summary>
        public bool CaseInsensitive { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output enum as prefix free.
        /// </summary>
        public bool EnumPrefixFree { get; set; }

        /// <summary>
        /// Gets/set a value indicating the navigation property depth.
        /// </summary>
        public bool NavigationPropertyPathItem { get; set; }

        /// <summary>
        /// Gets/set a value indicating the prefix for the parameter alias.
        /// </summary>
        public string ParameterAlias { get; set; } = "@p";

        /// <summary>
        /// Gets/sets a value indicating to set the OperationId on <see cref="OpenApiOperation"/>.
        /// </summary>
        public bool OperationId { get; set; } = true;

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
    }
}
