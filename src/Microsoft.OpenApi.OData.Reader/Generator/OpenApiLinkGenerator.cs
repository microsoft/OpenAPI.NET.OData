// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiLink"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiLinkGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiLink"/> object.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The Entity Set.</param>
        /// <returns>The created dictionary of <see cref="OpenApiLink"/> object.</returns>
        public static IDictionary<string, OpenApiLink> CreateLinks(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            IDictionary<string, OpenApiLink> links = new Dictionary<string, OpenApiLink>();
            IEdmEntityType entityType = entitySet.EntityType();
            foreach (var np in entityType.DeclaredNavigationProperties())
            {
                OpenApiLink link = new OpenApiLink();
                string typeName = entitySet.EntityType().Name;
                link.OperationId = entitySet.Name + "." + typeName + ".Get" + Utils.UpperFirstChar(typeName);
                link.Parameters = new Dictionary<string, RuntimeExpressionAnyWrapper>();
                foreach (var key in entityType.Key())
                {
                    link.Parameters[key.Name] = new RuntimeExpressionAnyWrapper
                    {
                        Any = new OpenApiString("$request.path." + key.Name)
                    };
                }

                links[np.Name] = link;
            }

            return links;
        }
    }
}
