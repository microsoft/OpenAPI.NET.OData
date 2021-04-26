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
    /// Base class for entity set operation.
    /// </summary>
    internal abstract class EntitySetOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets/sets the <see cref="IEdmEntitySet"/>.
        /// </summary>
        protected IEdmEntitySet EntitySet { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // get the entity set.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;

            EntitySet = navigationSourceSegment.NavigationSource as IEdmEntitySet;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            // Description
            operation.Description = Context.Model.GetDescriptionAnnotation(EntitySet);

            base.SetBasicInfo(operation);
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            OpenApiTag tag = new OpenApiTag
            {
                Name = EntitySet.Name + "." + EntitySet.EntityType().Name,
            };

            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));

            operation.Tags.Add(tag);

            Context.AppendTag(tag);

            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiString("operation"));

            base.SetExtensions(operation);
        }
    }
}
