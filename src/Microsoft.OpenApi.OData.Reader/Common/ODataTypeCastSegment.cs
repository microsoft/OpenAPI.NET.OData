// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Type cast segment
    /// </summary>
    public class ODataTypeCastSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataTypeCastSegment"/> class.
        /// </summary>
        /// <param name="entityType"></param>
        public ODataTypeCastSegment(IEdmEntityType entityType)
        {
            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
        }

        public override IEdmEntityType EntityType
        {
            get;
        }

        public override string ToString()
        {
            return EntityType.FullTypeName();
        }
    }
}