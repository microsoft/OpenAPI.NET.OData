// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiTag"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiTagGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiTag"/> object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert settings.</param>
        /// <returns>The created collection of <see cref="OpenApiTag"/> object.</returns>
        public static IList<OpenApiTag> CreateTags(this IEdmModel model, OpenApiConvertSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (settings == null)
            {
                throw Error.ArgumentNull(nameof(settings));
            }

            // The value of tags is an array of Tag Objects.
            // For an OData service the natural groups are entity sets and singletons,
            // so the tags array contains one Tag Object per entity set and singleton in the entity container.

            // A Tag Object has to contain the field name, whose value is the name of the entity set or singleton,
            // and it optionally can contain the field description, whose value is the value of the unqualified annotation
            // Core.Description of the entity set or singleton.
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
                            OpenApiTag actionImportTag = model.CreateOperationImportTag((IEdmActionImport)element);
                            if (actionImportTag != null)
                            {
                                tags.Add(actionImportTag);
                            }
                            break;

                        case EdmContainerElementKind.FunctionImport: // Function Import
                            OpenApiTag functionImportTag = model.CreateOperationImportTag((IEdmFunctionImport)element);
                            if (functionImportTag != null)
                            {
                                tags.Add(functionImportTag);
                            }
                            break;
                    }
                }
            }

            // Tags is optional.
            return tags.Any() ? tags : null;
        }

        private static OpenApiTag CreateOperationImportTag(this IEdmModel model, IEdmOperationImport operationImport)
        {
            if (operationImport == null || operationImport.EntitySet != null)
            {
                return null;
            }

            return new OpenApiTag
            {
                Name = operationImport.Name,
                Description = model.GetDescription(operationImport)
            };
        }
    }
}
