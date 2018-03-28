// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Capabilities;
using System;

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
            Utils.CheckArgumentNull(context, nameof(context));

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
                        // entity set
                        string entitySetPathName = "/" + entitySet.Name;
                        var entitySetPathItem = context.CreateEntitySetPathItem(entitySet);
                        pathItems.Add(entitySetPathName, entitySetPathItem);

                        // entity
                        string entityPathName = context.CreateEntityPathName(entitySet);
                        var entityPathItem = context.CreateEntityPathItem(entitySet);
                        pathItems.Add(entityPathName, entityPathItem);

                        // navigation property
                        foreach (var item in context.CreateNavigationPathItems(entitySet))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }

                        // bound operations to entity set or entity
                        foreach (var item in context.CreateOperationPathItems(entitySet))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }

                        break;

                    case EdmContainerElementKind.Singleton: // singleton
                        IEdmSingleton singleton = (IEdmSingleton)element;
                        string singletonPathName = "/" + singleton.Name;
                        var singletonPathItem = context.CreateSingletonPathItem(singleton);
                        pathItems.Add(singletonPathName, singletonPathItem);

                        // navigation property
                        foreach (var item in context.CreateNavigationPathItems(singleton))
                        {
                            pathItems.Add(item.Key, item.Value);
                        }

                        // bound operations to singleton
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
        /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmEntitySet"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The Edm entity set.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/>.</returns>
        public static OpenApiPathItem CreateEntitySetPathItem(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            OpenApiPathItem pathItem = new OpenApiPathItem();

            pathItem.AddOperation(OperationType.Get, context.CreateEntitySetGetOperation(entitySet));

            InsertRestrictions insert = new InsertRestrictions(context.Model, entitySet);
            if (insert.IsInsertable())
            {
                pathItem.AddOperation(OperationType.Post, context.CreateEntitySetPostOperation(entitySet));
            }

            return pathItem;
        }

        /// <summary>
        /// Create a <see cref="OpenApiPathItem"/> for a single entity in <see cref="IEdmEntitySet"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The Edm entity set.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/>.</returns>
        public static OpenApiPathItem CreateEntityPathItem(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            OpenApiPathItem pathItem = new OpenApiPathItem();

            IndexableByKey index = new IndexableByKey(context.Model, entitySet);
            if (index.IsSupported())
            {
                pathItem.AddOperation(OperationType.Get, context.CreateEntityGetOperation(entitySet));
            }

            UpdateRestrictions update = new UpdateRestrictions(context.Model, entitySet);
            if (update.IsUpdatable())
            {
                pathItem.AddOperation(OperationType.Patch, context.CreateEntityPatchOperation(entitySet));
            }

            DeleteRestrictions delete = new DeleteRestrictions(context.Model, entitySet);
            if (delete.IsDeletable())
            {
                pathItem.AddOperation(OperationType.Delete, context.CreateEntityDeleteOperation(entitySet));
            }

            return pathItem;
        }

        /// <summary>
        /// Create a <see cref="OpenApiPathItem"/> for <see cref="IEdmSingleton"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="singleton">The singleton.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/> on this singleton.</returns>
        public static OpenApiPathItem CreateSingletonPathItem(this ODataContext context, IEdmSingleton singleton)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(singleton, nameof(singleton));

            OpenApiPathItem pathItem = new OpenApiPathItem();

            // Retrieve a singleton.
            pathItem.AddOperation(OperationType.Get, context.CreateSingletonGetOperation(singleton));

            // Update a singleton
            pathItem.AddOperation(OperationType.Patch, context.CreateSingletonPatchOperation(singleton));

            return pathItem;
        }

        /// <summary>
        /// Create the bound operations for the navigation source.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The name/value pairs describing the allowed operations on this navigation source.</returns>
        public static IDictionary<string, OpenApiPathItem> CreateOperationPathItems(this ODataContext context,
            IEdmNavigationSource navigationSource)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));

            IDictionary<string, OpenApiPathItem> operationPathItems = new Dictionary<string, OpenApiPathItem>();

            IEnumerable<Tuple<IEdmEntityType, IEdmOperation>> operations;
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;

            // collection bound
            if (entitySet != null)
            {
                operations = context.FindOperations(navigationSource.EntityType(), collection: true);
                foreach (var operation in operations)
                {
                    OpenApiPathItem pathItem = context.CreatePathItem(navigationSource, operation.Item1, operation.Item2);
                    string pathName = context.CreatePathItemName(operation.Item2);

                    // Append the type cast
                    if (!operation.Item1.IsEquivalentTo(navigationSource.EntityType()))
                    {
                        pathName = "/" + operation.Item1.FullTypeName() + pathName;
                    }
                    operationPathItems.Add("/" + navigationSource.Name + pathName, pathItem);
                }
            }

            // non-collection bound
            operations = context.FindOperations(navigationSource.EntityType(), collection: false);
            foreach (var operation in operations)
            {
                OpenApiPathItem pathItem = context.CreatePathItem(navigationSource, operation.Item1, operation.Item2);
                string pathName = context.CreatePathItemName(operation.Item2);

                string entityPathName;
                if (entitySet != null)
                {
                    entityPathName = context.CreateEntityPathName(entitySet);
                }
                else
                {
                    entityPathName = "/" + navigationSource.Name;
                }

                // Append the type cast
                if (!operation.Item1.IsEquivalentTo(navigationSource.EntityType()))
                {
                    pathName = "/" + operation.Item1.FullTypeName() + pathName;
                }
                operationPathItems.Add(entityPathName + pathName, pathItem);
            }

            return operationPathItems;
        }

        /// <summary>
        /// Create the navigation property path item for the navigation source.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The name/value pairs describing the allowed operations on this navigation source.</returns>
        public static IDictionary<string, OpenApiPathItem> CreateNavigationPathItems(this ODataContext context,
            IEdmNavigationSource navigationSource)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));

            IDictionary<string, OpenApiPathItem> navPathItems = new Dictionary<string, OpenApiPathItem>();

            IEdmEntityType entityType = navigationSource.EntityType();
            foreach (var navProperty in entityType.DeclaredNavigationProperties())
            {
                if (navProperty.ContainsTarget || context.Settings.NavigationPropertyPathItem)
                {
                    string pathItemName = context.CreateNavigationPathItemName(navigationSource, navProperty);
                    OpenApiPathItem pathItem = context.CreatePathItem(navigationSource, navProperty);

                    navPathItems.Add(pathItemName, pathItem);
                }
            }

            return navPathItems;
        }

        /// <summary>
        /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The binding navigation source.</param>
        /// <param name="navigationProperty">The Edm navigation property.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/>.</returns>
        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmNavigationSource navigationSource,
            IEdmNavigationProperty navigationProperty)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            OpenApiPathItem pathItem = new OpenApiPathItem();

            pathItem.AddOperation(OperationType.Get, context.CreateNavigationGetOperation(navigationSource, navigationProperty));

            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                InsertRestrictions insert = new InsertRestrictions(context.Model, navigationProperty);
                if (insert.IsInsertable())
                {
                    pathItem.AddOperation(OperationType.Post, context.CreateNavigationPostOperation(navigationSource, navigationProperty));
                }
            }
            else
            {
                UpdateRestrictions update = new UpdateRestrictions(context.Model, navigationProperty);
                if (update.IsUpdatable())
                {
                    pathItem.AddOperation(OperationType.Patch, context.CreateNavigationPatchOperation(navigationSource, navigationProperty));
                }
            }

            return pathItem;
        }

        /// <summary>
        /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmOperation"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The binding navigation source.</param>
        /// <param name="edmOperation">The Edm opeation.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/>.</returns>
        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmNavigationSource navigationSource, IEdmEntityType entityType, IEdmOperation edmOperation)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(edmOperation, nameof(edmOperation));

            OpenApiPathItem pathItem = new OpenApiPathItem();

            OpenApiOperation operation = context.CreateOperation(navigationSource, entityType, edmOperation);

            if (edmOperation.IsAction())
            {
                // The Path Item Object for a bound action contains the keyword post,
                // The value of the operation keyword is an Operation Object that describes how to invoke the action.
                pathItem.AddOperation(OperationType.Post, operation);
            }
            else
            {
                // The Path Item Object for a bound function contains the keyword get,
                // The value of the operation keyword is an Operation Object that describes how to invoke the function.
                pathItem.AddOperation(OperationType.Get, operation);
            }

            return pathItem;
        }

        /// <summary>
        /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmOperationImport"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>The created <see cref="OpenApiPathItem"/>.</returns>
        public static OpenApiPathItem CreatePathItem(this ODataContext context, IEdmOperationImport operationImport)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(operationImport, nameof(operationImport));

            OpenApiPathItem pathItem = new OpenApiPathItem();

            OpenApiOperation operation = context.CreateOperation(operationImport);

            if (operationImport.IsActionImport())
            {
                // Each action import is represented as a name/value pair whose name is the service-relative
                // resource path of the action import prepended with a forward slash, and whose value is a Path
                // Item Object containing the keyword post with an Operation Object as value that describes
                // how to invoke the action import.
                pathItem.AddOperation(OperationType.Post, operation);
            }
            else
            {
                // Each function import is represented as a name/value pair whose name is the service-relative
                // resource path of the function import prepended with a forward slash, and whose value is a Path
                // Item Object containing the keyword get with an Operation Object as value that describes
                // how to invoke the function import.
                pathItem.AddOperation(OperationType.Get, operation);
            }

            return pathItem;
        }
    }
}
