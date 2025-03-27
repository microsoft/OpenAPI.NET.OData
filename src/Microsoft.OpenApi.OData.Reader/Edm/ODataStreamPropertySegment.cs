//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Property Stream segment.
    /// </summary>
    public class ODataStreamPropertySegment : ODataSegment
    {
        private readonly string _streamPropertyName;
        /// <summary>
        /// Initializes a new instance of <see cref="ODataStreamPropertySegment"/> class.
        /// </summary>
        /// <param name="streamPropertyName">The name of the stream property.</param>
        public ODataStreamPropertySegment(string streamPropertyName)
        {
            _streamPropertyName = streamPropertyName ?? throw Error.ArgumentNull(nameof(streamPropertyName));
        }

        /// <inheritdoc />
        public override IEdmEntityType? EntityType => null;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.StreamProperty;

        /// <inheritdoc />
        public override string Identifier { get => _streamPropertyName; }

        /// <inheritdoc />
		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return Enumerable.Empty<IEdmVocabularyAnnotatable>();
		}

		/// <inheritdoc />
		public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => _streamPropertyName;
    }
}
