// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Net.Http;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class OperationHandlerProviderTests
    {
        [Theory]
        [InlineData(ODataPathKind.EntitySet, "get", typeof(EntitySetGetOperationHandler))]
        [InlineData(ODataPathKind.EntitySet, "post", typeof(EntitySetPostOperationHandler))]
        [InlineData(ODataPathKind.Entity, "get", typeof(EntityGetOperationHandler))]
        [InlineData(ODataPathKind.Entity, "patch", typeof(EntityPatchOperationHandler))]
        [InlineData(ODataPathKind.Entity, "delete", typeof(EntityDeleteOperationHandler))]
        [InlineData(ODataPathKind.Singleton, "get", typeof(SingletonGetOperationHandler))]
        [InlineData(ODataPathKind.Singleton, "patch", typeof(SingletonPatchOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, "get", typeof(NavigationPropertyGetOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, "post", typeof(NavigationPropertyPostOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, "patch", typeof(NavigationPropertyPatchOperationHandler))]
        [InlineData(ODataPathKind.NavigationProperty, "delete", typeof(NavigationPropertyDeleteOperationHandler))]
        [InlineData(ODataPathKind.Operation, "get", typeof(EdmFunctionOperationHandler))]
        [InlineData(ODataPathKind.Operation, "post", typeof(EdmActionOperationHandler))]
        [InlineData(ODataPathKind.OperationImport, "get", typeof(EdmFunctionImportOperationHandler))]
        [InlineData(ODataPathKind.OperationImport, "post", typeof(EdmActionImportOperationHandler))]
        [InlineData(ODataPathKind.Ref, "post", typeof(RefPostOperationHandler))]
        [InlineData(ODataPathKind.Ref, "delete", typeof(RefDeleteOperationHandler))]
        [InlineData(ODataPathKind.Ref, "get", typeof(RefGetOperationHandler))]
        [InlineData(ODataPathKind.Ref, "put", typeof(RefPutOperationHandler))]
        [InlineData(ODataPathKind.MediaEntity, "get", typeof(MediaEntityGetOperationHandler))]
        [InlineData(ODataPathKind.MediaEntity, "put", typeof(MediaEntityPutOperationHandler))]
        [InlineData(ODataPathKind.Metadata, "get", typeof(MetadataGetOperationHandler))]
        [InlineData(ODataPathKind.DollarCount, "get", typeof(DollarCountGetOperationHandler))]
        public void GetHandlerReturnsCorrectOperationHandlerType(ODataPathKind pathKind, string operationType, Type handlerType)
        {
            // Arrange
            OpenApiDocument openApiDocument = new();
            OperationHandlerProvider provider = new OperationHandlerProvider();

            // Act
            IOperationHandler handler = provider.GetHandler(pathKind, HttpMethod.Parse(operationType), openApiDocument);

            // Assert
            Assert.Same(handlerType, handler.GetType());
        }
    }
}
