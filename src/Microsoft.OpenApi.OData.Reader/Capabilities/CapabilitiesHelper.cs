// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    internal static class CapabilitiesHelper
    {
        /// <summary>
        /// Gets boolean value for term Org.OData.Core.V1.KeyAsSegmentSupported
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <returns>Boolean for term Org.OData.Core.V1.KeyAsSegmentSupported</returns>
        public static bool GetKeyAsSegmentSupported(this IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            if (model.EntityContainer == null)
            {
                return false;
            }

            KeyAsSegmentSupported keyAsSegment = new KeyAsSegmentSupported(model, model.EntityContainer);
            return keyAsSegment.Supported ?? false;
        }
    }
}
