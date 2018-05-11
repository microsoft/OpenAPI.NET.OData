// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
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
        /// <param name="navigaitonSource">The navigation source.</param>
        public ODataNavigationSourceSegment(IEdmNavigationSource navigaitonSource)
        {
            NavigationSource = navigaitonSource ?? throw Error.ArgumentNull(nameof(navigaitonSource));
        }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override IEdmEntityType EntityType => NavigationSource.EntityType();

        /// <inheritdoc />
        public override string Name => NavigationSource.Name;

        /// <inheritdoc />
        public override string ToString()
        {
            return NavigationSource.Name;
        }
    }
}