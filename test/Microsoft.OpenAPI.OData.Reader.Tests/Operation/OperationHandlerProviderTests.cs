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
        [InlineData(ODataPathType.EntitySet, OperationType.Get, typeof(EntitySetGetOperationHandler))]
        [InlineData(ODataPathType.EntitySet, OperationType.Post, typeof(EntitySetPostOperationHandler))]
        [InlineData(ODataPathType.Entity, OperationType.Get, typeof(EntityGetOperationHandler))]
        [InlineData(ODataPathType.Entity, OperationType.Patch, typeof(EntityPatchOperationHandler))]
        [InlineData(ODataPathType.Entity, OperationType.Delete, typeof(EntityDeleteOperationHandler))]
        [InlineData(ODataPathType.Singleton, OperationType.Get, typeof(SingletonGetOperationHandler))]
        [InlineData(ODataPathType.Singleton, OperationType.Patch, typeof(SingletonPatchOperationHandler))]
        [InlineData(ODataPathType.NavigationProperty, OperationType.Get, typeof(NavigationPropertyGetOperationHandler))]
        [InlineData(ODataPathType.NavigationProperty, OperationType.Post, typeof(NavigationPropertyPostOperationHandler))]
        [InlineData(ODataPathType.NavigationProperty, OperationType.Patch, typeof(NavigationPropertyPatchOperationHandler))]
        [InlineData(ODataPathType.Operation, OperationType.Get, typeof(EdmFunctionOperationHandler))]
        [InlineData(ODataPathType.Operation, OperationType.Post, typeof(EdmActionOperationHandler))]
        [InlineData(ODataPathType.OperationImport, OperationType.Get, typeof(EdmFunctionImportOperationHandler))]
        [InlineData(ODataPathType.OperationImport, OperationType.Post, typeof(EdmActionImportOperationHandler))]
        public void GetHandlerReturnsCorrectOperationHandlerType(ODataPathType pathType, OperationType operationType, Type handlerType)
        {
            // Arrange
            OperationHandlerProvider provider = new OperationHandlerProvider();

            // Act
            IOperationHandler hander = provider.GetHandler(pathType, operationType);

            // Assert
            Assert.Same(handlerType, hander.GetType());
        }
    }
}
