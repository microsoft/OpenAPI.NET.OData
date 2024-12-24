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
        private static Dictionary<ODataPathKind, IDictionary<OperationType, IOperationHandler>> GetHandlers(OpenApiDocument document)
            => new()
            {

            // entity set (Get/Post)
            {ODataPathKind.EntitySet, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EntitySetGetOperationHandler(document) },
                {OperationType.Post, new EntitySetPostOperationHandler(document) }
            }},

            // entity (Get/Patch/Put/Delete)
            {ODataPathKind.Entity, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EntityGetOperationHandler(document) },
                {OperationType.Patch, new EntityPatchOperationHandler(document) },
                {OperationType.Put, new EntityPutOperationHandler(document) },
                {OperationType.Delete, new EntityDeleteOperationHandler(document) }
            }},

            // singleton (Get/Patch)
            {ODataPathKind.Singleton, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new SingletonGetOperationHandler(document) },
                {OperationType.Patch, new SingletonPatchOperationHandler(document) }
            }},

            // edm operation (Get|Post)
            {ODataPathKind.Operation, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EdmFunctionOperationHandler(document) },
                {OperationType.Post, new EdmActionOperationHandler(document) }
            }},

            // edm operation import (Get|Post)
            {ODataPathKind.OperationImport, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new EdmFunctionImportOperationHandler(document) },
                {OperationType.Post, new EdmActionImportOperationHandler(document) }
            }},

            // navigation property (Get/Patch/Put/Post/Delete)
            {ODataPathKind.NavigationProperty, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new NavigationPropertyGetOperationHandler(document) },
                {OperationType.Patch, new NavigationPropertyPatchOperationHandler(document) },
                {OperationType.Put, new NavigationPropertyPutOperationHandler(document) },
                {OperationType.Post, new NavigationPropertyPostOperationHandler(document) },
                {OperationType.Delete, new NavigationPropertyDeleteOperationHandler(document) }
            }},

            // navigation property ref (Get/Post/Put/Delete)
            {ODataPathKind.Ref, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new RefGetOperationHandler(document) },
                {OperationType.Put, new RefPutOperationHandler(document) },
                {OperationType.Post, new RefPostOperationHandler(document) },
                {OperationType.Delete, new RefDeleteOperationHandler(document) }
            }},

            // media entity operation (Get|Put|Delete)
            {ODataPathKind.MediaEntity, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new MediaEntityGetOperationHandler(document) },
                {OperationType.Put, new MediaEntityPutOperationHandler(document) },
                {OperationType.Delete, new MediaEntityDeleteOperationHandler(document) }
            }},

            // $metadata operation (Get)
            {ODataPathKind.Metadata, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new MetadataGetOperationHandler(document) }
            }},

            // $count operation (Get)
            {ODataPathKind.DollarCount, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new DollarCountGetOperationHandler(document) }
            }},

            // .../namespace.typename (cast, get)
            {ODataPathKind.TypeCast, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new ODataTypeCastGetOperationHandler(document) },
            }},

            // .../entity/propertyOfComplexType (Get/Patch/Put/Delete)
            {ODataPathKind.ComplexProperty, new Dictionary<OperationType, IOperationHandler>
            {
                {OperationType.Get, new ComplexPropertyGetOperationHandler(document) },
                {OperationType.Patch, new ComplexPropertyPatchOperationHandler(document) },
                {OperationType.Put, new ComplexPropertyPutOperationHandler(document) },
                {OperationType.Post, new ComplexPropertyPostOperationHandler(document) }
            }}
        };

        /// <inheritdoc/>
        public IOperationHandler GetHandler(ODataPathKind pathKind, OperationType operationType)
        {
            //TODO refactor to avoid dictionary creation on each call
            return GetHandlers(document)[pathKind][operationType];
        }
    }
}
