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
        [InlineData(ODataPathType.EntitySet, typeof(EntitySetPathItemHandler))]
        [InlineData(ODataPathType.Entity, typeof(EntityPathItemHandler))]
        [InlineData(ODataPathType.Singleton, typeof(SingletonPathItemHandler))]
        [InlineData(ODataPathType.NavigationProperty, typeof(NavigationPropertyPathItemHandler))]
        [InlineData(ODataPathType.Operation, typeof(OperationPathItemHandler))]
        [InlineData(ODataPathType.OperationImport, typeof(OperationImportPathItemHandler))]
        public void GetHandlerReturnsCorrectHandlerType(ODataPathType pathType, Type handlerType)
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
