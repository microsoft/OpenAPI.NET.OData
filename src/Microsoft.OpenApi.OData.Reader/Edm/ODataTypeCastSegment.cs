// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
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
        public override string Identifier { get => EntityType.FullTypeName(); }

        /// <inheritdoc />
		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return new IEdmVocabularyAnnotatable[] { EntityType };
		}

		/// <inheritdoc />
		public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => EntityType.FullTypeName();
    }
}