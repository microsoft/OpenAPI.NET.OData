// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Segment kind.
    /// </summary>
    public enum ODataSegmentKind
    {
        /// <summary>
        /// Navigation source (entity set or singleton )
        /// </summary>
        NavigationSource,

        /// <summary>
        /// Navigation property
        /// </summary>
        NavigationProperty,

        /// <summary>
        /// Edm bound operation (function or action)
        /// </summary>
        Operation,

        /// <summary>
        /// Edm unbound operation (function import or action import)
        /// </summary>
        OperationImport,

        /// <summary>
        /// Key
        /// </summary>
        Key,

        /// <summary>
        /// Type cast
        /// </summary>
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
        /// Gets the name of this segment.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the path item name for this segment.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The path item name.</returns>
        public abstract string GetPathItemName(OpenApiConvertSettings settings);
    }
}
