// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Convert settings.
    /// </summary>
    public class OpenApiConvertSettings
    {
        public Uri ServiceRoot { get; set; } = new Uri("http://localhost");

        public Version Version { get; set; } = new Version(1, 0, 1);

        public bool KeyAsSegment { get; set; }

        public bool UnqualifiedCall { get; set; }

        public bool CaseInsensitive { get; set; }

        public bool EnumPrefixFree { get; set; }

        public int MaxNavPropertyPathDepth { get; set; }

        public string ParameterAlias { get; set; } = "@p";

        /// <summary>
        /// Gets/sets a value indicating to set the OperationId on <see cref="OpenApiOperation"/>.
        /// </summary>
        public bool OperationId { get; set; } = false;
    }
}
