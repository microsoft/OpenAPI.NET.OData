// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

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
                (ODataPathKind.EntitySet, OperationType.Get) => new EntitySetGetOperationHandler(),
                (ODataPathKind.EntitySet, OperationType.Post) => new EntitySetPostOperationHandler(),

                // entity (Get/Patch/Put/Delete)
                (ODataPathKind.Entity, OperationType.Get) => new EntityGetOperationHandler(),
                (ODataPathKind.Entity, OperationType.Patch) => new EntityPatchOperationHandler(),
                (ODataPathKind.Entity, OperationType.Put) => new EntityPutOperationHandler(),
                (ODataPathKind.Entity, OperationType.Delete) => new EntityDeleteOperationHandler(),

                // singleton (Get/Patch)
                (ODataPathKind.Singleton, OperationType.Get) => new SingletonGetOperationHandler(),
                (ODataPathKind.Singleton, OperationType.Patch) => new SingletonPatchOperationHandler(),

                // edm operation (Get|Post)
                (ODataPathKind.Operation, OperationType.Get) => new EdmFunctionOperationHandler(),
                (ODataPathKind.Operation, OperationType.Post) => new EdmActionOperationHandler(),

                // edm operation import (Get|Post)
                (ODataPathKind.OperationImport, OperationType.Get) => new EdmFunctionImportOperationHandler(),
                (ODataPathKind.OperationImport, OperationType.Post) => new EdmActionImportOperationHandler(),

                // navigation property (Get/Patch/Put/Post/Delete)
                (ODataPathKind.NavigationProperty, OperationType.Get) => new NavigationPropertyGetOperationHandler(),
                (ODataPathKind.NavigationProperty, OperationType.Patch) => new NavigationPropertyPatchOperationHandler(),
                (ODataPathKind.NavigationProperty, OperationType.Put) => new NavigationPropertyPutOperationHandler(),
                (ODataPathKind.NavigationProperty, OperationType.Post) => new NavigationPropertyPostOperationHandler(),
                (ODataPathKind.NavigationProperty, OperationType.Delete) => new NavigationPropertyDeleteOperationHandler(),

                // navigation property ref (Get/Post/Put/Delete)
                (ODataPathKind.Ref, OperationType.Get) => new RefGetOperationHandler(),
                (ODataPathKind.Ref, OperationType.Put) => new RefPutOperationHandler(),
                (ODataPathKind.Ref, OperationType.Post) => new RefPostOperationHandler(),
                (ODataPathKind.Ref, OperationType.Delete) => new RefDeleteOperationHandler(),

                // media entity operation (Get|Put|Delete)
                (ODataPathKind.MediaEntity, OperationType.Get) => new MediaEntityGetOperationHandler(),
                (ODataPathKind.MediaEntity, OperationType.Put) => new MediaEntityPutOperationHandler(),
                (ODataPathKind.MediaEntity, OperationType.Delete) => new MediaEntityDeleteOperationHandler(),

                // $metadata operation (Get)

                (ODataPathKind.Metadata, OperationType.Get) => new MetadataGetOperationHandler(),

                // $count operation (Get)
                (ODataPathKind.DollarCount, OperationType.Get) => new DollarCountGetOperationHandler(),

                // .../namespace.typename (cast, get)
                (ODataPathKind.TypeCast, OperationType.Get) => new ODataTypeCastGetOperationHandler(),

                // .../entity/propertyOfComplexType (Get/Patch/Put/Delete)
                (ODataPathKind.ComplexProperty, OperationType.Get) => new ComplexPropertyGetOperationHandler(),
                (ODataPathKind.ComplexProperty, OperationType.Patch) => new ComplexPropertyPatchOperationHandler(),
                (ODataPathKind.ComplexProperty, OperationType.Put) => new ComplexPropertyPutOperationHandler(),
                (ODataPathKind.ComplexProperty, OperationType.Post) => new ComplexPropertyPostOperationHandler(),

                (_, _) => null,
            };
        }
    }
}
