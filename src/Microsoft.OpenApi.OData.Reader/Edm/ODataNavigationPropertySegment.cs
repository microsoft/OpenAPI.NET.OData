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
    /// Navigation property segment.
    /// </summary>
    public class ODataNavigationPropertySegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataNavigationPropertySegment"/> class.
        /// </summary>
        /// <param name="navigationProperty">The Navigation property</param>
        public ODataNavigationPropertySegment(IEdmNavigationProperty navigationProperty)
        {
            NavigationProperty = navigationProperty ?? throw Error.ArgumentNull(nameof(navigationProperty));
        }

        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty { get; }

        /// <inheritdoc />
        public override IEdmEntityType EntityType => NavigationProperty.ToEntityType();

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.NavigationProperty;

        /// <inheritdoc />
        public override string Identifier { get => NavigationProperty.Name; }

		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return new IEdmVocabularyAnnotatable[] { NavigationProperty, EntityType };
		}

		/// <inheritdoc />
		public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => NavigationProperty.Name;
    }
}