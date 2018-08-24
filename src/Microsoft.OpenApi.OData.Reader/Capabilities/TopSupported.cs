// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.TopSupported
    /// </summary>
    internal class TopSupported : SupportedRestrictions
    {
        /// <summary>
        /// The Term type Kind.
        /// </summary>
        public override CapabilitesTermKind Kind => CapabilitesTermKind.TopSupported;
    }
}
