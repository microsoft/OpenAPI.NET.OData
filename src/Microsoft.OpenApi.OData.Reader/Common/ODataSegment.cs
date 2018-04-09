// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Represents a OData segment. For example, an entity set segment.
    /// </summary>
    public abstract class ODataSegment
    {
        /// <summary>
        /// Gets the entity type of current segment.
        /// </summary>
        public abstract IEdmEntityType EntityType { get; }
    }
}