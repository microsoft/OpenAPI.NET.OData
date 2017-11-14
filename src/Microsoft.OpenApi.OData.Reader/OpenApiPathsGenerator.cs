// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiPaths"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiPathsGenerator
    {
        /// <summary>
        /// Create the <see cref="OpenApiPaths"/>
        /// The value of paths is a Paths Object.
        /// It is the main source of information on how to use the described API.
        /// It consists of name/value pairs whose name is a path template relative to the service root URL,
        /// and whose value is a Path Item Object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>the paths object.</returns>
        public static OpenApiPaths CreatePaths(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            // Due to the power and flexibility of OData a full representation of all service capabilities
            // in the Paths Object is typically not feasible, so this mapping only describes the minimum
            // information desired in the Paths Object.
            OpenApiPaths paths = new OpenApiPaths();
            if (model.EntityContainer != null)
            {
                IDictionary<IEdmTypeReference, IEdmOperation> boundOperations = new Dictionary<IEdmTypeReference, IEdmOperation>();
                foreach (var edmOperation in model.SchemaElements.OfType<IEdmOperation>().Where(e => e.IsBound))
                {
                    IEdmOperationParameter bindingParameter = edmOperation.Parameters.First();
                    boundOperations.Add(bindingParameter.Type, edmOperation);
                }

                foreach (var element in model.EntityContainer.Elements)
                {
                    switch (element.ContainerElementKind)
                    {
                        case EdmContainerElementKind.EntitySet: // entity set
                            IEdmEntitySet entitySet = element as IEdmEntitySet;
                            if (entitySet != null)
                            {
                                foreach (var item in entitySet.CreatePathItems())
                                {
                                    paths.Add(item.Key, item.Value);
                                }

                                foreach(var item in entitySet.CreateOperationPathItems(boundOperations))
                                {
                                    paths.Add(item.Key, item.Value);
                                }
                            }
                            break;

                        case EdmContainerElementKind.Singleton: // singleton
                            IEdmSingleton singleton = element as IEdmSingleton;
                            if (singleton != null)
                            {
                                foreach (var item in singleton.CreatePathItems())
                                {
                                    paths.Add(item.Key, item.Value);
                                }

                                foreach (var item in singleton.CreateOperationPathItems(boundOperations))
                                {
                                    paths.Add(item.Key, item.Value);
                                }
                            }
                            break;

                        case EdmContainerElementKind.FunctionImport: // function import
                            IEdmFunctionImport functionImport = element as IEdmFunctionImport;
                            if (functionImport != null)
                            {
                                var functionImportPathItem = functionImport.CreatePathItem();

                                paths.Add(functionImport.CreatePathItemName(), functionImportPathItem);
                            }
                            break;

                        case EdmContainerElementKind.ActionImport: // action import
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
