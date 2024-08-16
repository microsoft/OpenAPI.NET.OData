// ------------------------------------------------------------
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
    /// Navigation source (entity set or singleton) segment.
    /// </summary>
    public class ODataNavigationSourceSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataNavigationSourceSegment"/> class.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        public ODataNavigationSourceSegment(IEdmNavigationSource navigationSource)
        {
            NavigationSource = navigationSource ?? throw Error.ArgumentNull(nameof(navigationSource));
        }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override IEdmEntityType EntityType => NavigationSource.EntityType;

        /// <inheritdoc />
        public override string Identifier { get => NavigationSource.Name; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.NavigationSource;

        /// <inheritdoc />
		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return new IEdmVocabularyAnnotatable[] { NavigationSource as IEdmVocabularyAnnotatable, EntityType }.Union(EntityType.FindAllBaseTypes());
		}

		/// <inheritdoc />
		public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => NavigationSource.Name;
    }
}