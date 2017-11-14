// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generators
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiPathItem"/> by Edm elements.
    /// </summary>
    internal static class OpenApiPathItemGenerator
    {
        /// <summary>
        /// Path items for Entity Sets.
        /// Each entity set is represented as a name/value pair
        /// whose name is the service-relative resource path of the entity set prepended with a forward slash,
        /// and whose value is a Path Item Object.
        /// </summary>
        /// <param name="entitySet">The Edm entity set.</param>
        /// <returns>The path items.</returns>
        public static IDictionary<string, OpenApiPathItem> CreatePathItems(this IEdmEntitySet entitySet)
        {
            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            IDictionary<string, OpenApiPathItem> paths = new Dictionary<string, OpenApiPathItem>();

            // entity set
            OpenApiPathItem pathItem = new OpenApiPathItem();

            pathItem.AddOperation(OperationType.Get, entitySet.CreateGetOperationForEntitySet());

            pathItem.AddOperation(OperationType.Post, entitySet.CreatePostOperationForEntitySet());

            paths.Add("/" + entitySet.Name, pathItem);

            // entity
            string entityPathName = entitySet.CreatePathNameForEntity();
            pathItem = new OpenApiPathItem();

            pathItem.AddOperation(OperationType.Get, entitySet.CreateGetOperationForEntity());

            pathItem.AddOperation(OperationType.Patch, entitySet.CreatePatchOperationForEntity());

            pathItem.AddOperation(OperationType.Delete, entitySet.CreateDeleteOperationForEntity());

            paths.Add(entityPathName, pathItem);

            return paths;
        }

        public static IDictionary<string, OpenApiPathItem> CreatePathItems(this IEdmSingleton singleton)
        {
            if (singleton == null)
            {
                throw Error.ArgumentNull(nameof(singleton));
            }

            IDictionary<string, OpenApiPathItem> paths = new Dictionary<string, OpenApiPathItem>();

            // Singleton
            string entityPathName = singleton.CreatePathNameForSingleton();
            OpenApiPathItem pathItem = new OpenApiPathItem();
            pathItem.AddOperation(OperationType.Get, singleton.CreateGetOperationForSingleton());
            pathItem.AddOperation(OperationType.Patch, singleton.CreatePatchOperationForSingleton());
            paths.Add(entityPathName, pathItem);

            return paths;
        }

        public static IDictionary<string, OpenApiPathItem> CreateOperationPathItems(this IEdmNavigationSource navigationSource,
            IDictionary<IEdmTypeReference, IEdmOperation> boundOperations)
        {
            IDictionary<string, OpenApiPathItem> operationPathItems = new Dictionary<string, OpenApiPathItem>();

            IEnumerable<IEdmOperation> operations;
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            // collection bound
            if (entitySet != null)
            {
                operations = FindOperations(navigationSource.EntityType(), boundOperations, collection: true);
                foreach (var operation in operations)
                {
                    OpenApiPathItem openApiOperation = operation.CreatePathItem();
                    string operationPathName = operation.CreatePathItemName();
                    operationPathItems.Add("/" + navigationSource.Name + operationPathName, openApiOperation);
                }
            }

            // non-collection bound
            operations = FindOperations(navigationSource.EntityType(), boundOperations, collection: false);
            foreach (var operation in operations)
            {
                OpenApiPathItem openApiOperation = operation.CreatePathItem();
                string operationPathName = operation.CreatePathItemName();

                string temp;
                if (entitySet != null)
                {
                    temp = entitySet.CreatePathNameForEntity();
                }
                else
                {
                    temp = "/" + navigationSource.Name;
                }
                operationPathItems.Add(temp + operationPathName, openApiOperation);
            }

            return operationPathItems;
        }

        private static IEnumerable<IEdmOperation> FindOperations(IEdmEntityType entityType,
            IDictionary<IEdmTypeReference, IEdmOperation> boundOperations,
            bool collection)
        {
            string fullTypeName = collection ? "Collection(" + entityType.FullName() + ")" :
                entityType.FullName();

            foreach (var item in boundOperations)
            {
                if (item.Key.FullName() == fullTypeName)
                {
                    yield return item.Value;
                }
            }
        }

        public static OpenApiPathItem CreatePathItem(this IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return ((IEdmActionImport)operationImport).CreatePathItem();
            }

            return ((IEdmFunctionImport)operationImport).CreatePathItem();
        }

        public static OpenApiPathItem CreatePathItem(this IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return ((IEdmAction)operation).CreatePathItem();
            }

            return ((IEdmFunction)operation).CreatePathItem();
        }

        public static OpenApiPathItem CreatePathItem(this IEdmActionImport actionImport)
        {
            return CreatePathItem(actionImport.Action);
        }

        public static OpenApiPathItem CreatePathItem(this IEdmAction action)
        {
            OpenApiPathItem pathItem = new OpenApiPathItem();

            OpenApiOperation post = new OpenApiOperation
            {
                Summary = "Invoke action " + action.Name,
                Tags = CreateTags(action),
                Parameters = action.CreateParameters(),
                Responses = action.CreateResponses()
            };

            pathItem.AddOperation(OperationType.Post, post);
            return pathItem;
        }

        public static OpenApiPathItem CreatePathItem(this IEdmFunctionImport functionImport)
        {
            return CreatePathItem(functionImport.Function);
        }

        public static OpenApiPathItem CreatePathItem(this IEdmFunction function)
        {
            OpenApiPathItem pathItem = new OpenApiPathItem();
            OpenApiOperation get = new OpenApiOperation
            {
                Summary = "Invoke function " + function.Name,
                Tags = CreateTags(function),
                Parameters = function.CreateParameters(),
                Responses = function.CreateResponses()
            };

            pathItem.AddOperation(OperationType.Get, get);
            return pathItem;
        }

        public static string CreatePathItemName(this IEdmActionImport actionImport)
        {
            return CreatePathItemName(actionImport.Action);
        }

        public static string CreatePathItemName(this IEdmAction action)
        {
            return "/" + action.Name;
        }

        public static string CreatePathItemName(this IEdmFunctionImport functionImport)
        {
            return CreatePathItemName(functionImport.Function);
        }

        public static string CreatePathItemName(this IEdmFunction function)
        {
            StringBuilder functionName = new StringBuilder("/" + function.Name + "(");

            functionName.Append(String.Join(",",
                function.Parameters.Select(p => p.Name + "=" + "{" + p.Name + "}")));
            functionName.Append(")");

            return functionName.ToString();
        }

        public static string CreatePathItemName(this IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return ((IEdmActionImport)operationImport).CreatePathItemName();
            }

            return ((IEdmFunctionImport)operationImport).CreatePathItemName();
        }

        public static string CreatePathItemName(this IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return ((IEdmAction)operation).CreatePathItemName();
            }

            return ((IEdmFunction)operation).CreatePathItemName();
        }

        private static IList<string> CreateTags(this IEdmOperationImport operationImport)
        {
            if (operationImport.EntitySet != null)
            {
                var pathExpression = operationImport.EntitySet as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<string>
                    {
                        PathAsString(pathExpression.PathSegments)
                    };
                }
            }

            return null;
        }

        private static IList<OpenApiTag> CreateTags(this IEdmOperation operation)
        {
            if (operation.EntitySetPath != null)
            {
                var pathExpression = operation.EntitySetPath as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<OpenApiTag>
                    {
                        new OpenApiTag
                        {
                            Name = PathAsString(pathExpression.PathSegments)
                        }
                    };
                }
            }

            return null;
        }

        private static IList<OpenApiParameter> CreateParameters(this IEdmOperation operation)
        {
            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            foreach (IEdmOperationParameter edmParameter in operation.Parameters)
            {
                parameters.Add(new OpenApiParameter
                {
                    Name = edmParameter.Name,
                    In = ParameterLocation.Path,
                    Required = true,
                    Schema = edmParameter.Type.CreateSchema()
                });
            }

            return parameters;
        }

        internal static string PathAsString(IEnumerable<string> path)
        {
            return String.Join("/", path);
        }
    }
}
