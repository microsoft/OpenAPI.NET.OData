// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.KeyAsSegmentSupported
    /// </summary>
    internal class KeyAsSegmentSupported : SupportedRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.KeyAsSegmentSupported;

        /// <summary>
        /// Initializes a new instance of <see cref="TopSupported"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public KeyAsSegmentSupported(IEdmModel model, IEdmEntityContainer target)
            : base(model, target)
        {
        }
    }
}
