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
        /// <param name="structuredType">The target type cast type.</param>
        public ODataTypeCastSegment(IEdmStructuredType structuredType)
        {
            StructuredType = structuredType ?? throw Error.ArgumentNull(nameof(structuredType));
        }
        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.TypeCast;

        /// <inheritdoc />
        public override string Identifier { get => StructuredType.FullTypeName(); }

        /// <summary>
        /// Gets the target type cast type.
        /// </summary>
		public IEdmStructuredType StructuredType { get; private set; }

		/// <inheritdoc />
		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return new IEdmVocabularyAnnotatable[] { StructuredType as IEdmVocabularyAnnotatable };
		}

		/// <inheritdoc />
		public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => StructuredType.FullTypeName();
    }
}