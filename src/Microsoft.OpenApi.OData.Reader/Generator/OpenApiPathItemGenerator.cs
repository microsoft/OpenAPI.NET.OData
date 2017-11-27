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

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Class to create <see cref="OpenApiPathItem"/> by Edm elements.
    /// </summary>
    internal static class OpenApiPathItemGenerator
    {
        /// <summary>
        /// Create a map of <see cref="OpenApiPathItem"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <returns>The created map of <see cref="OpenApiPathItem"/>.</returns>
        public static IDictionary<string, OpenApiPathItem> CreatePathItems(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IDictionary<string, OpenApiPathItem> pathItems = new Dictionary<string, OpenApiPathItem>();
            if (context.EntityContainer == null)
            {
                return pathItems;
            }

            // visit all elements in the container
            foreach (var element in context.EntityContainer.Elements)
            {
                switch (element.ContainerElementKind)
                {
                    case EdmContainerElementKind.EntitySet: // entity set
                        IEdmEntitySet entitySet = (IEdmEntitySet)element;
                        foreach (var item in context.CreatePathItems(entitySet))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }

                        foreach (var item in context.CreateOperationPathItems(entitySet))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }
                        break;

                    case EdmContainerElementKind.Singleton: // singleton
                        IEdmSingleton singleton = (IEdmSingleton)element;
                        foreach (var item in context.CreatePathItems(singleton))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }

                        foreach (var item in context.CreateOperationPathItems(singleton))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }
                        break;

                    case EdmContainerElementKind.FunctionImport: // function import
                        IEdmFunctionImport functionImport = (IEdmFunctionImport)element;
                        string functionImportName = context.CreatePathItemName(functionImport);
                        var functionImportPathItem = context.CreatePathItem(functionImport);
                        pathItems.Add(functionImportName, functionImportPathItem);
                        break;

                    case EdmContainerElementKind.ActionImport: // action import
                        IEdmActionImport actionImport = (IEdmActionImport)element;
                        string actionImportName = context.CreatePathItemName(actionImport);
                        var actionImportPathItem = context.CreatePathItem(actionImport);
                        pathItems.Add(actionImportName, actionImportPathItem);
                        break;
                }
            }

            return pathItems;
        }

        /// <summary>
        /// Path items for Entity Sets.
        /// Each entity set is represented as a name/value pair
        /// whose name is the service-relative resource path of the entity set prepended with a forward slash,
        /// and whose value is a Path Item Object.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The Edm entity set.</param>
        /// <returns>The path items.</returns>
        public static IDictionary<string, OpenApiPathItem> CreatePathItems(this ODataContext context, IEdmEntitySet entitySet)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (entitySet == null)
            {
                throw Error.ArgumentNull(nameof(entitySet));
            }

            IDictionary<string, OpenApiPathItem> paths = new Dictionary<string, OpenApiPathItem>();

            // entity set
            OpenApiPathItem pathItem = new OpenApiPathItem();

            pathItem.AddOperation(OperationType.Get, context.CreateEntitySetGetOperation(entitySet));

            pathItem.AddOperation(OperationType.Post, context.CreateEntitySetPostOperation(entitySet));

            paths.Add("/" + entitySet.Name, pathItem);

            // entity
            string entityPathName = entitySet.CreatePathNameForEntity();
            pathItem = new OpenApiPathItem();

            pathItem.AddOperation(OperationType.Get, context.CreateEntityGetOperation(entitySet));

            pathItem.AddOperation(OperationType.Patch, context.CreateEntityPatchOperation(entitySet));

            pathItem.AddOperation(OperationType.Delete, context.CreateEntityDeleteOperation(entitySet));

            paths.Add(entityPathName, pathItem);

            return paths;
        }

        /// <summary>
        /// Each singleton is represented as a name/value pair whose name is the service-relative resource
        /// path of the singleton prepended with a forward slash, whose value is <see cref="OpenApiPathItem"/>
        /// describing the allowed operations on this singleton.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The name/value pairs describing the allowed operations on this singleton.</returns>
        private static IDictionary<string, OpenApiPathItem> CreatePathItems(this ODataContext context, IEdmSingleton singleton)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (singleton == null)
            {
                throw Error.ArgumentNull(nameof(singleton));
            }

            IDictionary<string, OpenApiPathItem> paths = new Dictionary<string, OpenApiPathItem>();

            // Singleton
            string entityPathName = singleton.CreatePathNameForSingleton();
            OpenApiPathItem pathItem = new OpenApiPathItem();

            // Retrieve a singleton.
            pathItem.AddOperation(OperationType.Get, context.CreateSingletonGetOperation(singleton));

            // Update a singleton
            pathItem.AddOperation(OperationType.Patch, context.CreateSingletonPatchOperation(singleton));

            paths.Add(entityPathName, pathItem);

            return paths;
        }

        /// <summary>
        /// Create the bound operations for the navigation source.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The name/value pairs describing the allowed operations on this navigation source.</returns>
        public static IDictionary<string, OpenApiPathItem> CreateOperationPathItems(this ODataContext context, IEdmNavigationSource navigationSource)
        {
            IDictionary<string, OpenApiPathItem> operationPathItems = new Dictionary<string, OpenApiPathItem>();

            IEnumerable<IEdmOperation> operations;
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            // collection bound
            if (entitySet != null)
            {
                operations = context.FindOperations(navigationSource.EntityType(), collection: true);
                foreach (var operation in operations)
                {
                    OpenApiPathItem openApiOperation = context.CreatePathItem(operation);
                    string operationPathName = context.CreatePathItemName(operation);
                    operationPathItems.Add("/" + navigationSource.Name + operationPathName, openApiOperation);
                }
            }

            // non-collection bound
            operations = context.FindOperations(navigationSource.EntityType(), collection: false);
            foreach (var operation in operations)
            {
                OpenApiPathItem openApiOperation = context.CreatePathItem(operation);
                string operationPathName = context.CreatePathItemName(operation);

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

        public static string CreatePathNameForEntity(this IEdmEntitySet entitySet)
        {
            string keyString;
            IList<IEdmStructuralProperty> keys = entitySet.EntityType().Key().ToList();
            if (keys.Count() == 1)
            {
                keyString = "{" + keys.First().Name + "}";
            }
            else
            {
                IList<string> temps = new List<string>();
                foreach (var keyProperty in entitySet.EntityType().Key())
                {
                    temps.Add(keyProperty.Name + "={" + keyProperty.Name + "}");
                }
                keyString = String.Join(",", temps);
            }

            return "/" + entitySet.Name + "('" + keyString + "')";
        }

        public static string CreatePathNameForSingleton(this IEdmSingleton singleton)
        {
            return "/" + singleton.Name;
        }
        private static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return context.CreatePathItem((IEdmActionImport)operationImport);
            }

            return context.CreatePathItem((IEdmFunctionImport)operationImport);
        }

        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmActionImport actionImport)
        {
            return context.CreatePathItem(actionImport.Action);
        }

        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmFunctionImport functionImport)
        {
            return context.CreatePathItem(functionImport.Function);
        }


        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return context.CreatePathItem((IEdmAction)operation);
            }

            return context.CreatePathItem((IEdmFunction)operation);
        }

        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmAction action)
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

        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmFunction function)
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

        public static string CreatePathItemName(this ODataContext context, IEdmActionImport actionImport)
        {
            return context.CreatePathItemName(actionImport.Action);
        }

        public static string CreatePathItemName(this ODataContext context, IEdmAction action)
        {
            return "/" + action.Name;
        }

        private static string CreatePathItemName(this ODataContext context, IEdmFunctionImport functionImport)
        {
            return context.CreatePathItemName(functionImport.Function);
        }

        public static string CreatePathItemName(this ODataContext context, IEdmFunction function)
        {
            StringBuilder functionName = new StringBuilder("/" + function.Name + "(");

            functionName.Append(String.Join(",",
                function.Parameters.Select(p => p.Name + "=" + "{" + p.Name + "}")));
            functionName.Append(")");

            return functionName.ToString();
        }

        public static string CreatePathItemName(this ODataContext context, IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return context.CreatePathItemName((IEdmActionImport)operationImport);
            }

            return context.CreatePathItemName((IEdmFunctionImport)operationImport);
        }

        public static string CreatePathItemName(this ODataContext context, IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return context.CreatePathItemName((IEdmAction)operation);
            }

            return context.CreatePathItemName((IEdmFunction)operation);
        }

        private static IList<string> CreateTags(this ODataContext context, IEdmOperationImport operationImport)
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

        private static IList<OpenApiTag> CreateTags(IEdmOperation operation)
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

        internal static string PathAsString(IEnumerable<string> path)
        {
            return String.Join("/", path);
        }
    }
}
