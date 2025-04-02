// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
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
        public IOperationHandler? GetHandler(ODataPathKind pathKind, HttpMethod operationType, OpenApiDocument document)
        {
            return (pathKind, operationType.ToString().ToLowerInvariant()) switch
            {
                // entity set (Get/Post)
                (ODataPathKind.EntitySet, "get") => new EntitySetGetOperationHandler(document),
                (ODataPathKind.EntitySet, "post") => new EntitySetPostOperationHandler(document),

                // entity (Get/Patch/Put/Delete)
                (ODataPathKind.Entity, "get") => new EntityGetOperationHandler(document),
                (ODataPathKind.Entity, "patch") => new EntityPatchOperationHandler(document),
                (ODataPathKind.Entity, "put") => new EntityPutOperationHandler(document),
                (ODataPathKind.Entity, "delete") => new EntityDeleteOperationHandler(document),

                // singleton (Get/Patch)
                (ODataPathKind.Singleton, "get") => new SingletonGetOperationHandler(document),
                (ODataPathKind.Singleton, "patch") => new SingletonPatchOperationHandler(document),

                // edm operation (Get|Post)
                (ODataPathKind.Operation, "get") => new EdmFunctionOperationHandler(document),
                (ODataPathKind.Operation, "post") => new EdmActionOperationHandler(document),

                // edm operation import (Get|Post)
                (ODataPathKind.OperationImport, "get") => new EdmFunctionImportOperationHandler(document),
                (ODataPathKind.OperationImport, "post") => new EdmActionImportOperationHandler(document),

                // navigation property (Get/Patch/Put/Post/Delete)
                (ODataPathKind.NavigationProperty, "get") => new NavigationPropertyGetOperationHandler(document),
                (ODataPathKind.NavigationProperty, "patch") => new NavigationPropertyPatchOperationHandler(document),
                (ODataPathKind.NavigationProperty, "put") => new NavigationPropertyPutOperationHandler(document),
                (ODataPathKind.NavigationProperty, "post") => new NavigationPropertyPostOperationHandler(document),
                (ODataPathKind.NavigationProperty, "delete") => new NavigationPropertyDeleteOperationHandler(document),

                // navigation property ref (Get/Post/Put/Delete)
                (ODataPathKind.Ref, "get") => new RefGetOperationHandler(document),
                (ODataPathKind.Ref, "put") => new RefPutOperationHandler(document),
                (ODataPathKind.Ref, "post") => new RefPostOperationHandler(document),
                (ODataPathKind.Ref, "delete") => new RefDeleteOperationHandler(document),

                // media entity operation (Get|Put|Delete)
                (ODataPathKind.MediaEntity, "get") => new MediaEntityGetOperationHandler(document),
                (ODataPathKind.MediaEntity, "put") => new MediaEntityPutOperationHandler(document),
                (ODataPathKind.MediaEntity, "delete") => new MediaEntityDeleteOperationHandler(document),

                // $metadata operation (Get)

                (ODataPathKind.Metadata, "get") => new MetadataGetOperationHandler(document),

                // $count operation (Get)
                (ODataPathKind.DollarCount, "get") => new DollarCountGetOperationHandler(document),

                // .../namespace.typename (cast, get)
                (ODataPathKind.TypeCast, "get") => new ODataTypeCastGetOperationHandler(document),

                // .../entity/propertyOfComplexType (Get/Patch/Put/Delete)
                (ODataPathKind.ComplexProperty, "get") => new ComplexPropertyGetOperationHandler(document),
                (ODataPathKind.ComplexProperty, "patch") => new ComplexPropertyPatchOperationHandler(document),
                (ODataPathKind.ComplexProperty, "put") => new ComplexPropertyPutOperationHandler(document),
                (ODataPathKind.ComplexProperty, "post") => new ComplexPropertyPostOperationHandler(document),

                (_, _) => null,
            };
        }
    }
}
