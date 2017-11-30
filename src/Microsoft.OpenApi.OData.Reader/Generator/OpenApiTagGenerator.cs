// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="context">The OData context.</param>
        /// <returns>The created collection of <see cref="OpenApiTag"/> object.</returns>
        public static IList<OpenApiTag> CreateTags(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // The value of tags is an array of Tag Objects.
            // For an OData service the natural groups are entity sets and singletons,
            // so the tags array contains one Tag Object per entity set and singleton in the entity container.

            // A Tag Object has to contain the field name, whose value is the name of the entity set or singleton,
            // and it optionally can contain the field description, whose value is the value of the unqualified annotation
            // Core.Description of the entity set or singleton.
            IList<OpenApiTag> tags = new List<OpenApiTag>();
            if (context.EntityContainer != null)
            {
                foreach (IEdmEntityContainerElement element in context.Model.EntityContainer.Elements)
                {
                    switch (element.ContainerElementKind)
                    {
                        case EdmContainerElementKind.EntitySet: // entity set
                            IEdmEntitySet entitySet = (IEdmEntitySet)element;
                            tags.Add(new OpenApiTag
                            {
                                Name = entitySet.Name,
                                Description = context.Model.GetDescriptionAnnotation(entitySet)
                            });
                            break;

                        case EdmContainerElementKind.Singleton: // singleton
                            IEdmSingleton singleton = (IEdmSingleton)element;
                            tags.Add(new OpenApiTag
                            {
                                Name = singleton.Name,
                                Description = context.Model.GetDescriptionAnnotation(singleton)
                            });
                            break;

                        // The tags array can contain additional Tag Objects for other logical groups,
                        // e.g. for action imports or function imports that are not associated with an entity set.
                        case EdmContainerElementKind.ActionImport: // Action Import
                            OpenApiTag actionImportTag = context.CreateOperationImportTag((IEdmActionImport)element);
                            if (actionImportTag != null)
                            {
                                tags.Add(actionImportTag);
                            }
                            break;

                        case EdmContainerElementKind.FunctionImport: // Function Import
                            OpenApiTag functionImportTag = context.CreateOperationImportTag((IEdmFunctionImport)element);
                            if (functionImportTag != null)
                            {
                                tags.Add(functionImportTag);
                            }
                            break;
                    }
                }
            }

            return tags;
        }

        private static OpenApiTag CreateOperationImportTag(this ODataContext context, IEdmOperationImport operationImport)
        {
            Debug.Assert(context != null);

            if (operationImport == null || operationImport.EntitySet != null)
            {
                return null;
            }

            return new OpenApiTag
            {
                Name = operationImport.Name,
                Description = context.Model.GetDescriptionAnnotation(operationImport)
            };
        }
    }
}
