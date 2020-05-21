// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// A class to provide the <see cref="IOperationHandler"/>.
    /// </summary>
    internal class OperationHandlerProvider : IOperationHandlerProvider
    {
        private IDictionary<ODataPathKind, IDictionary<OperationType, IOperationHandler>> _handlers;

        /// <summary>
        /// Initializes a new instance of <see cref="OperationHandlerProvider"/> class.
        /// </summary>
        public OperationHandlerProvider()
        {
            _handlers = new Dictionary<ODataPathKind, IDictionary<OperationType, IOperationHandler>>();

            // entity set (Get/Post)
            _handlers[ODataPathKind.EntitySet] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EntitySetGetOperationHandler() },
                {OperationType.Post, new EntitySetPostOperationHandler() }
            };

            // entity (Get/Patch/Delete)
            _handlers[ODataPathKind.Entity] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EntityGetOperationHandler() },
                {OperationType.Patch, new EntityPatchOperationHandler() },
                {OperationType.Delete, new EntityDeleteOperationHandler() }
            };

            // singleton (Get/Patch)
            _handlers[ODataPathKind.Singleton] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new SingletonGetOperationHandler() },
                {OperationType.Patch, new SingletonPatchOperationHandler() }
            };

            // edm operation (Get|Post)
            _handlers[ODataPathKind.Operation] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EdmFunctionOperationHandler() },
                {OperationType.Post, new EdmActionOperationHandler() }
            };

            // edm operation import (Get|Post)
            _handlers[ODataPathKind.OperationImport] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EdmFunctionImportOperationHandler() },
                {OperationType.Post, new EdmActionImportOperationHandler() }
            };

            // navigation property (Get/Patch/Post/Delete)
            _handlers[ODataPathKind.NavigationProperty] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new NavigationPropertyGetOperationHandler() },
                {OperationType.Patch, new NavigationPropertyPatchOperationHandler() },
                {OperationType.Post, new NavigationPropertyPostOperationHandler() },
                {OperationType.Delete, new NavigationPropertyDeleteOperationHandler() }
            };

            // navigatoin property ref (Get/Post/Patch/Delete)
            _handlers[ODataPathKind.Ref] = new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new RefGetOperationHandler() },
                {OperationType.Patch, new RefPatchOperationHandler() },
                {OperationType.Post, new RefPostOperationHandler() },
                {OperationType.Delete, new RefDeleteOperationHandler() }
            };
        }

        /// <inheritdoc/>
        public IOperationHandler GetHandler(ODataPathKind pathKind, OperationType operationType)
        {
            return _handlers[pathKind][operationType];
        }
    }
}
