// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.BatchSupported
    /// </summary>
    internal class BatchSupported : SupportedRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.BatchSupported;

        /// <summary>
        /// Initializes a new instance of <see cref="BatchSupported"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The annotation target <see cref="IEdmEntityContainer"/>.</param>
        public BatchSupported(IEdmModel model, IEdmEntityContainer target)
            : base(model, target)
        {
        }
    }
}
