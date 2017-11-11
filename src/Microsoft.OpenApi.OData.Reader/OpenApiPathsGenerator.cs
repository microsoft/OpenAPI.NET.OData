//---------------------------------------------------------------------
// <copyright file="EdmOpenApiPathsGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Visit Edm model to generate <see cref="OpenApiPaths"/>
    /// </summary>
    internal class OpenApiPathsGenerator
    {
        private OpenApiPathItemGenerator _nsGenerator;
        private IEdmModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiPathsGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        public OpenApiPathsGenerator(IEdmModel model)
        {
            _model = model;
            _nsGenerator = new OpenApiPathItemGenerator(model);
        }

        /// <summary>
        /// Create the <see cref="OpenApiPaths"/>
        /// </summary>
        /// <returns>the paths object.</returns>
        public OpenApiPaths Generate()
        {
            OpenApiPaths paths = new OpenApiPaths();
            if (_model.EntityContainer != null)
            {
                foreach (var element in _model.EntityContainer.Elements)
                {
                    switch (element.ContainerElementKind)
                    {
                        case EdmContainerElementKind.EntitySet:
                            IEdmEntitySet entitySet = element as IEdmEntitySet;
                            if (entitySet != null)
                            {
                                foreach (var item in _nsGenerator.CreatePaths(entitySet))
                                {
                                    paths.Add(item.Key, item.Value);
                                }
                            }
                            break;

                        case EdmContainerElementKind.Singleton:
                            IEdmSingleton singleton = element as IEdmSingleton;
                            if (singleton != null)
                            {
                                foreach (var item in _nsGenerator.CreatePaths(singleton))
                                {
                                    paths.Add(item.Key, item.Value);
                                }
                            }
                            break;

                        case EdmContainerElementKind.FunctionImport:
                            IEdmFunctionImport functionImport = element as IEdmFunctionImport;
                            if (functionImport != null)
                            {
                                var functionImportPathItem = functionImport.CreatePathItem();

                                paths.Add(functionImport.CreatePathItemName(), functionImportPathItem);
                            }
                            break;

                        case EdmContainerElementKind.ActionImport:
                            IEdmActionImport actionImport = element as IEdmActionImport;
                            if (actionImport != null)
                            {
                                var functionImportPathItem = actionImport.CreatePathItem();
                                paths.Add(actionImport.CreatePathItemName(), functionImportPathItem);
                            }
                            break;
                    }
                }
            }

            return paths;
        }
    }
}
