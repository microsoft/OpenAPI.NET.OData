// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Authorization
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal class OpenIDConnect : Authorization
    {
        public string IssuerUrl { get; set; }
    }
}