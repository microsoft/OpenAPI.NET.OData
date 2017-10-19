//---------------------------------------------------------------------
// <copyright file="EdmOpenApiPathsGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Visit Edm model to generate <see cref="OpenApiPaths"/>
    /// </summary>
    internal class EdmOpenApiPathsGenerator : EdmOpenApiGenerator
    {
        private OpenApiPaths _paths;
        private EdmNavigationSourceGenerator _nsGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmOpenApiPathsGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        public EdmOpenApiPathsGenerator(IEdmModel model, OpenApiWriterSettings settings)
            : base(model, settings)
        {
            _nsGenerator = new EdmNavigationSourceGenerator(model, settings);
        }

        /// <summary>
        /// Create the <see cref="OpenApiPaths"/>
        /// </summary>
        /// <returns>the paths object.</returns>
        public OpenApiPaths Generate()
        {
            if (_paths == null)
            {
                _paths = new OpenApiPaths();

                if (Model.EntityContainer != null)
                {
                    foreach (var element in Model.EntityContainer.Elements)
                    {
                        switch (element.ContainerElementKind)
                        {
                            case EdmContainerElementKind.EntitySet:
                                IEdmEntitySet entitySet = element as IEdmEntitySet;
                                if (entitySet != null)
                                {
                                    foreach (var item in _nsGenerator.CreatePaths(entitySet))
                                    {
                                        _paths.Add(item.Key, item.Value);
                                    }
                                }
                                break;

                            case EdmContainerElementKind.Singleton:
                                IEdmSingleton singleton = element as IEdmSingleton;
                                if (singleton != null)
                                {
                                    foreach (var item in _nsGenerator.CreatePaths(singleton))
                                    {
                                        _paths.Add(item.Key, item.Value);
                                    }
                                }
                                break;

                            case EdmContainerElementKind.FunctionImport:
                                IEdmFunctionImport functionImport = element as IEdmFunctionImport;
                                if (functionImport != null)
                                {
                                    var functionImportPathItem = functionImport.CreatePathItem();

                                    _paths.Add(functionImport.CreatePathItemName(), functionImportPathItem);
                                }
                                break;

                            case EdmContainerElementKind.ActionImport:
                                IEdmActionImport actionImport = element as IEdmActionImport;
                                if (actionImport != null)
                                {
                                    var functionImportPathItem = actionImport.CreatePathItem();
                                    _paths.Add(actionImport.CreatePathItemName(), functionImportPathItem);
                                }
                                break;
                        }
                    }
                }
            }

            return _paths;
        }
    }
}
