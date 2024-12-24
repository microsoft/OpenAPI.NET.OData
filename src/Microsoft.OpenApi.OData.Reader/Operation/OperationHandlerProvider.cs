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
        /// <inheritdoc/>
        public IOperationHandler GetHandler(ODataPathKind pathKind, OperationType operationType)
        {
            return (pathKind, operationType) switch
            {
                // entity set (Get/Post)
                (ODataPathKind.EntitySet, OperationType.Get) => new EntitySetGetOperationHandler(document),
                (ODataPathKind.EntitySet, OperationType.Post) => new EntitySetPostOperationHandler(document),

                // entity (Get/Patch/Put/Delete)
                (ODataPathKind.Entity, OperationType.Get) => new EntityGetOperationHandler(document),
                (ODataPathKind.Entity, OperationType.Patch) => new EntityPatchOperationHandler(document),
                (ODataPathKind.Entity, OperationType.Put) => new EntityPutOperationHandler(document),
                (ODataPathKind.Entity, OperationType.Delete) => new EntityDeleteOperationHandler(document),

                // singleton (Get/Patch)
                (ODataPathKind.Singleton, OperationType.Get) => new SingletonGetOperationHandler(document),
                (ODataPathKind.Singleton, OperationType.Patch) => new SingletonPatchOperationHandler(document),

                // edm operation (Get|Post)
                (ODataPathKind.Operation, OperationType.Get) => new EdmFunctionOperationHandler(document),
                (ODataPathKind.Operation, OperationType.Post) => new EdmActionOperationHandler(document),

                // edm operation import (Get|Post)
                (ODataPathKind.OperationImport, OperationType.Get) => new EdmFunctionImportOperationHandler(document),
                (ODataPathKind.OperationImport, OperationType.Post) => new EdmActionImportOperationHandler(document),

                // navigation property (Get/Patch/Put/Post/Delete)
                (ODataPathKind.NavigationProperty, OperationType.Get) => new NavigationPropertyGetOperationHandler(document),
                (ODataPathKind.NavigationProperty, OperationType.Patch) => new NavigationPropertyPatchOperationHandler(document),
                (ODataPathKind.NavigationProperty, OperationType.Put) => new NavigationPropertyPutOperationHandler(document),
                (ODataPathKind.NavigationProperty, OperationType.Post) => new NavigationPropertyPostOperationHandler(document),
                (ODataPathKind.NavigationProperty, OperationType.Delete) => new NavigationPropertyDeleteOperationHandler(document),

                // navigation property ref (Get/Post/Put/Delete)
                (ODataPathKind.Ref, OperationType.Get) => new RefGetOperationHandler(document),
                (ODataPathKind.Ref, OperationType.Put) => new RefPutOperationHandler(document),
                (ODataPathKind.Ref, OperationType.Post) => new RefPostOperationHandler(document),
                (ODataPathKind.Ref, OperationType.Delete) => new RefDeleteOperationHandler(document),

                // media entity operation (Get|Put|Delete)
                (ODataPathKind.MediaEntity, OperationType.Get) => new MediaEntityGetOperationHandler(document),
                (ODataPathKind.MediaEntity, OperationType.Put) => new MediaEntityPutOperationHandler(document),
                (ODataPathKind.MediaEntity, OperationType.Delete) => new MediaEntityDeleteOperationHandler(document),

                // $metadata operation (Get)

                (ODataPathKind.Metadata, OperationType.Get) => new MetadataGetOperationHandler(document),

                // $count operation (Get)
                (ODataPathKind.DollarCount, OperationType.Get) => new DollarCountGetOperationHandler(document),

                // .../namespace.typename (cast, get)
                (ODataPathKind.TypeCast, OperationType.Get) => new ODataTypeCastGetOperationHandler(document),

                // .../entity/propertyOfComplexType (Get/Patch/Put/Delete)
                (ODataPathKind.ComplexProperty, OperationType.Get) => new ComplexPropertyGetOperationHandler(document),
                (ODataPathKind.ComplexProperty, OperationType.Patch) => new ComplexPropertyPatchOperationHandler(document),
                (ODataPathKind.ComplexProperty, OperationType.Put) => new ComplexPropertyPutOperationHandler(document),
                (ODataPathKind.ComplexProperty, OperationType.Post) => new ComplexPropertyPostOperationHandler(document),

                (_, _) => null,
            };
        }
    }
}
