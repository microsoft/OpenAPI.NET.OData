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
        [InlineData(PathType.EntitySet, OperationType.Get, typeof(EntitySetGetOperationHandler))]
        [InlineData(PathType.EntitySet, OperationType.Post, typeof(EntitySetPostOperationHandler))]
        [InlineData(PathType.Entity, OperationType.Get, typeof(EntityGetOperationHandler))]
        [InlineData(PathType.Entity, OperationType.Patch, typeof(EntityPatchOperationHandler))]
        [InlineData(PathType.Entity, OperationType.Delete, typeof(EntityDeleteOperationHandler))]
        [InlineData(PathType.Singleton, OperationType.Get, typeof(SingletonGetOperationHandler))]
        [InlineData(PathType.Singleton, OperationType.Patch, typeof(SingletonPatchOperationHandler))]
        [InlineData(PathType.NavigationProperty, OperationType.Get, typeof(NavigationPropertyGetOperationHandler))]
        [InlineData(PathType.NavigationProperty, OperationType.Post, typeof(NavigationPropertyPostOperationHandler))]
        [InlineData(PathType.NavigationProperty, OperationType.Patch, typeof(NavigationPropertyPatchOperationHandler))]
        [InlineData(PathType.Operation, OperationType.Get, typeof(EdmFunctionOperationHandler))]
        [InlineData(PathType.Operation, OperationType.Post, typeof(EdmActionOperationHandler))]
        [InlineData(PathType.OperationImport, OperationType.Get, typeof(EdmFunctionImportOperationHandler))]
        [InlineData(PathType.OperationImport, OperationType.Post, typeof(EdmActionImportOperationHandler))]
        public void GetHandlerReturnsCorrectOperationHandlerType(PathType pathType, OperationType operationType, Type handlerType)
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
