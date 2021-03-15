// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class OperationHandlerProviderTests
    {
        [Theory]
        [InlineData(ODataPathKind.EntitySet, OperationType.Get, typeof(EntitySetGetOperationHandler))]
        [InlineData(ODataPathKind.EntitySet, OperationType.Post, typeof(EntitySetPostOperationHandler))]
        [InlineData(ODataPathKind.Entity, OperationType.Get, typeof(EntityGetOperationHandler))]
        [InlineData(ODataPathKind.Entity, OperationType.Patch, typeof(EntityPatchOperationHandler))]
        [InlineData(ODataPathKind.Entity, OperationType.Delete, typeof(EntityDeleteOperationHandler))]
        [InlineData(ODataPathKind.Singleton, OperationType.Get, typeof(SingletonGetOperationHandler))]
        [InlineData(ODataPathKind.Singleton, OperationType.Patch, typeof(SingletonPatchOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, OperationType.Get, typeof(NavigationPropertyGetOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, OperationType.Post, typeof(NavigationPropertyPostOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, OperationType.Patch, typeof(NavigationPropertyPatchOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, OperationType.Delete, typeof(NavigationPropertyDeleteOperationHandler))]
        [InlineData(ODataPathKind.Operation, OperationType.Get, typeof(EdmFunctionOperationHandler))]
        [InlineData(ODataPathKind.Operation, OperationType.Post, typeof(EdmActionOperationHandler))]
        [InlineData(ODataPathKind.OperationImport, OperationType.Get, typeof(EdmFunctionImportOperationHandler))]
        [InlineData(ODataPathKind.OperationImport, OperationType.Post, typeof(EdmActionImportOperationHandler))]
        [InlineData(ODataPathKind.Ref, OperationType.Post, typeof(RefPostOperationHandler))]
        [InlineData(ODataPathKind.Ref, OperationType.Delete, typeof(RefDeleteOperationHandler))]
        [InlineData(ODataPathKind.Ref, OperationType.Get, typeof(RefGetOperationHandler))]
        [InlineData(ODataPathKind.Ref, OperationType.Put, typeof(RefPutOperationHandler))]
        [InlineData(ODataPathKind.MediaEntity, OperationType.Get, typeof(MediaEntityGetOperationHandler))]
        [InlineData(ODataPathKind.MediaEntity, OperationType.Put, typeof(MediaEntityPutOperationHandler))]
        [InlineData(ODataPathKind.Metadata, OperationType.Get, typeof(MetadataGetOperationHandler))]
        [InlineData(ODataPathKind.DollarCount, OperationType.Get, typeof(DollarCountGetOperationHandler))]
        public void GetHandlerReturnsCorrectOperationHandlerType(ODataPathKind pathKind, OperationType operationType, Type handlerType)
        {
            // Arrange
            OperationHandlerProvider provider = new OperationHandlerProvider();

            // Act
            IOperationHandler hander = provider.GetHandler(pathKind, operationType);

            // Assert
            Assert.Same(handlerType, hander.GetType());
        }
    }
}
