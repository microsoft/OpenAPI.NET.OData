// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generators
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiTag"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiTagsGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiTag"/> object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The collection of <see cref="OpenApiTag"/> object.</returns>
        public static IList<OpenApiTag> CreateTags(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            // The value of tags is an array of Tag Objects.
            // For an OData service the natural groups are entity sets and singletons,
            // so the tags array contains one Tag Object per entity set and singleton in the entity container.
            IList<OpenApiTag> tags = new List<OpenApiTag>();
            if (model.EntityContainer != null)
            {
                foreach (IEdmEntityContainerElement element in model.EntityContainer.Elements)
                {
                    switch (element.ContainerElementKind)
                    {
                        case EdmContainerElementKind.EntitySet: // entity set
                            IEdmEntitySet entitySet = (IEdmEntitySet)element;
                            tags.Add(new OpenApiTag
                            {
                                Name = entitySet.Name,
                                Description = model.GetDescription(entitySet)
                            });
                            break;

                        case EdmContainerElementKind.Singleton: // singleton
                            IEdmSingleton singleton = (IEdmSingleton)element;
                            tags.Add(new OpenApiTag
                            {
                                Name = singleton.Name,
                                Description = model.GetDescription(singleton)
                            });
                            break;

                        // The tags array can contain additional Tag Objects for other logical groups,
                        // e.g. for action imports or function imports that are not associated with an entity set.
                        case EdmContainerElementKind.ActionImport: // Action Import
                            break;
                        case EdmContainerElementKind.FunctionImport: // Function Import
                            break;
                    }
                }
            }

            // Tags is optional.
            return tags.Any() ? tags : null;
        }
    }
}
