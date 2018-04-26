// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class PathItemHandlerProviderTests
    {
        [Theory]
        [InlineData(PathType.EntitySet, typeof(EntitySetPathItemHandler))]
        [InlineData(PathType.Entity, typeof(EntityPathItemHandler))]
        [InlineData(PathType.Singleton, typeof(SingletonPathItemHandler))]
        [InlineData(PathType.NavigationProperty, typeof(NavigationPropertyPathItemHandler))]
        [InlineData(PathType.Operation, typeof(OperationPathItemHandler))]
        [InlineData(PathType.OperationImport, typeof(OperationImportPathItemHandler))]
        public void GetHandlerReturnsCorrectHandlerType(PathType pathType, Type handlerType)
        {
            // Arrange
            PathItemHandlerProvider provider = new PathItemHandlerProvider();

            // Act
            IPathItemHandler hander = provider.GetHandler(pathType);

            // Assert
            Assert.Same(handlerType, hander.GetType());
        }
    }
}
