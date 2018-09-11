// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;

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
            // get the entity set.
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            EntitySet = navigationSourceSegment.NavigationSource as IEdmEntitySet;
            base.Initialize(context, path);
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
        }

        /// <inheritdoc/>
        protected override void SetSecurity(OpenApiOperation operation)
        {
            base.SetSecurity(operation);

            var request = Context.FindRequest(EntitySet, OperationType.ToString());
            if (request != null)
            {
                operation.Security = Context.CreateSecurityRequirements(request.SecuritySchemes).ToList();
            }
        }

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            var request = Context.FindRequest(EntitySet, OperationType.ToString());
            if (request != null)
            {
                AppendCustomParameters(operation, request);
            }
        }
    }
}
