// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Type cast segment.
    /// </summary>
    public class ODataTypeCastSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataTypeCastSegment"/> class.
        /// </summary>
        /// <param name="entityType">The type cast type.</param>
        public ODataTypeCastSegment(IEdmEntityType entityType)
        {
            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
        }

        /// <inheritdoc />
        public override IEdmEntityType EntityType { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.TypeCast;

        /// <inheritdoc />
        public override string Name { get => EntityType.Name; }

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings) => EntityType.FullTypeName();
    }
}