// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Edm
{
    public enum ODataSegmentKind
    {
        NavigationSource,
        NavigationProperty,
        Operation,
        OperationImport,
        Key,
        TypeCast
    }

    /// <summary>
    /// Represents an OData segment. For example, an entity set segment.
    /// </summary>
    public abstract class ODataSegment
    {
        /// <summary>
        /// Gets the entity type of current segment.
        /// </summary>
        public virtual IEdmEntityType EntityType => throw new NotImplementedException();

        /// <summary>
        /// Gets the kind of this segment.
        /// </summary>
        public abstract ODataSegmentKind Kind { get; }

        /// <summary>
        /// Gets the path item name for this segment.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public abstract string GetPathItemName(OpenApiConvertSettings settings);
    }
}
