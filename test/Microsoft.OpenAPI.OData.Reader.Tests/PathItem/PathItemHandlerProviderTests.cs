// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class PathItemHandlerProviderTests
    {
        [Theory]
        [InlineData(ODataPathKind.EntitySet, typeof(EntitySetPathItemHandler))]
        [InlineData(ODataPathKind.Entity, typeof(EntityPathItemHandler))]
        [InlineData(ODataPathKind.Singleton, typeof(SingletonPathItemHandler))]
        [InlineData(ODataPathKind.NavigationProperty, typeof(NavigationPropertyPathItemHandler))]
        [InlineData(ODataPathKind.Operation, typeof(OperationPathItemHandler))]
        [InlineData(ODataPathKind.OperationImport, typeof(OperationImportPathItemHandler))]
        [InlineData(ODataPathKind.Ref, typeof(RefPathItemHandler))]
        [InlineData(ODataPathKind.MediaEntity, typeof(MediaEntityPathItemHandler))]
        [InlineData(ODataPathKind.Metadata, typeof(MetadataPathItemHandler))]
        [InlineData(ODataPathKind.DollarCount, typeof(DollarCountPathItemHandler))]
        [InlineData(ODataPathKind.TypeCast, typeof(ODataTypeCastPathItemHandler))]
        public void GetHandlerReturnsCorrectHandlerType(ODataPathKind pathKind, Type handlerType)
        {
            // Arrange
            PathItemHandlerProvider provider = new PathItemHandlerProvider();
            OpenApiDocument openApiDocument = new OpenApiDocument();

            // Act
            IPathItemHandler hander = provider.GetHandler(pathKind, openApiDocument);

            // Assert
            Assert.Same(handlerType, hander.GetType());
        }

        [Fact]
        public void GetHandlerReturnsNullForUnknownPathKind()
        {
            // Arrange
            PathItemHandlerProvider provider = new PathItemHandlerProvider();
            OpenApiDocument openApiDocument = new OpenApiDocument();

            // Act
            IPathItemHandler hander = provider.GetHandler(ODataPathKind.Unknown, openApiDocument);

            // Assert
            Assert.Null(hander);
        }
    }
}
