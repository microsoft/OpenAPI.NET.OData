// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class EdmOperationProviderTests
    {
        [Fact]
        public void CtorThrowArgumentNullModel()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => new EdmOperationProvider(model: null));
        }

        [Fact]
        public void FindOperationsReturnsCorrectCollectionOrOperations()
        {
            // Arrange
            var model = EdmModelHelper.GraphBetaModel;

            var provider = new EdmOperationProvider(model);

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("users");

            // Act
            var operations = provider.FindOperations(entitySet.EntityType, false);

            // Assert
            Assert.Equal(30, operations.Count());

            // Act
            entitySet = model.EntityContainer.FindEntitySet("directoryObjects");

            operations = provider.FindOperations(entitySet.EntityType, false);

            // Assert
            Assert.Equal(58, operations.Count());
        }
    }
}
