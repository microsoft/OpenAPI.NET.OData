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
        /// <param name="entityType">The Entity type.</param>
        /// <param name ="sourceElementName">The name of the source of the <see cref="IEdmEntityType".</param>
        /// <returns>The created dictionary of <see cref="OpenApiLink"/> object.</returns>
        public static IDictionary<string, OpenApiLink> CreateLinks(this ODataContext context, IEdmEntityType entityType, string sourceElementName)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entityType, nameof(entityType));
            Utils.CheckArgumentNullOrEmpty(sourceElementName, nameof(sourceElementName));

            IDictionary<string, OpenApiLink> links = new Dictionary<string, OpenApiLink>();
            foreach (IEdmNavigationProperty np in entityType.DeclaredNavigationProperties())
            {
                OpenApiLink link = new OpenApiLink
                {
                    OperationId = sourceElementName + "." + entityType.Name + ".Get" + Utils.UpperFirstChar(entityType.Name),
                    Parameters = new Dictionary<string, RuntimeExpressionAnyWrapper>()
                };

                foreach (IEdmStructuralProperty key in entityType.Key())
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
