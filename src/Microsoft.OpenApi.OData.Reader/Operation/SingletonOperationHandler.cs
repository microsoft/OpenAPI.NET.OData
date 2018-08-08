// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of <see cref="IEdmSingleton"/>.
    /// </summary>
    internal abstract class SingletonOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets the <see cref="IEdmSingleton"/>.
        /// </summary>
        protected IEdmSingleton Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;

            Singleton = navigationSourceSegment.NavigationSource as IEdmSingleton;

            base.Initialize(context, path);
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            OpenApiTag tag = new OpenApiTag
            {
                Name = Singleton.Name + "." + Singleton.EntityType().Name,
            };
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);
        }
    }
}
