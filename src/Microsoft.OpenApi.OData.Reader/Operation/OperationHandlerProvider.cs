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
        private readonly IDictionary<ODataPathKind, IDictionary<OperationType, IOperationHandler>> _handlers
            = new Dictionary<ODataPathKind, IDictionary<OperationType, IOperationHandler>>{

            // entity set (Get/Post)
            {ODataPathKind.EntitySet, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EntitySetGetOperationHandler() },
                {OperationType.Post, new EntitySetPostOperationHandler() }
            }},

            // entity (Get/Patch/Put/Delete)
            {ODataPathKind.Entity, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EntityGetOperationHandler() },
                {OperationType.Patch, new EntityPatchOperationHandler() },
                {OperationType.Put, new EntityPutOperationHandler() },
                {OperationType.Delete, new EntityDeleteOperationHandler() }
            }},

            // singleton (Get/Patch)
            {ODataPathKind.Singleton, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new SingletonGetOperationHandler() },
                {OperationType.Patch, new SingletonPatchOperationHandler() }
            }},

            // edm operation (Get|Post)
            {ODataPathKind.Operation, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EdmFunctionOperationHandler() },
                {OperationType.Post, new EdmActionOperationHandler() }
            }},

            // edm operation import (Get|Post)
            {ODataPathKind.OperationImport, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EdmFunctionImportOperationHandler() },
                {OperationType.Post, new EdmActionImportOperationHandler() }
            }},

            // navigation property (Get/Patch/Put/Post/Delete)
            {ODataPathKind.NavigationProperty, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new NavigationPropertyGetOperationHandler() },
                {OperationType.Patch, new NavigationPropertyPatchOperationHandler() },
                {OperationType.Put, new NavigationPropertyPutOperationHandler() },
                {OperationType.Post, new NavigationPropertyPostOperationHandler() },
                {OperationType.Delete, new NavigationPropertyDeleteOperationHandler() }
            }},

            // navigation property ref (Get/Post/Put/Delete)
            {ODataPathKind.Ref, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new RefGetOperationHandler() },
                {OperationType.Put, new RefPutOperationHandler() },
                {OperationType.Post, new RefPostOperationHandler() },
                {OperationType.Delete, new RefDeleteOperationHandler() }
            }},

            // media entity operation (Get|Put)
            {ODataPathKind.MediaEntity, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new MediaEntityGetOperationHandler() },
                {OperationType.Put, new MediaEntityPutOperationHandler() }
            }},

            // $metadata operation (Get)
            {ODataPathKind.Metadata, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new MetadataGetOperationHandler() }
            }},

            // $count operation (Get)
            {ODataPathKind.DollarCount, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new DollarCountGetOperationHandler() }
            }},

            // .../namespace.typename (cast, get)
            {ODataPathKind.TypeCast, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new ODataTypeCastGetOperationHandler() },
            }},

            // .../entity/propertyOfComplexType
            {ODataPathKind.ComplexProperty, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new ComplexPropertyGetOperationHandler() },
                {OperationType.Patch, new ComplexPropertyPatchOperationHandler() },
                {OperationType.Post, new ComplexPropertyPostOperationHandler() },
                {OperationType.Delete, new ComplexPropertyDeleteOperationHandler() },
            }},
        };

        /// <inheritdoc/>
        public IOperationHandler GetHandler(ODataPathKind pathKind, OperationType operationType)
        {
            return _handlers[pathKind][operationType];
        }
    }
}
