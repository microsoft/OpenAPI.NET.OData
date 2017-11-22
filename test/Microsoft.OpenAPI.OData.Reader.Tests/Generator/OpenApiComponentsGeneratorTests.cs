// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiComponentsGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CreateComponentsThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => model.CreateComponents(_settings));
        }

        [Fact]
        public void CreateComponentsThrowArgumentNullSettings()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => model.CreateComponents(settings: null));
        }

        [Fact]
        public void CreateComponentsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            var components = model.CreateComponents(_settings);

            // Assert
            Assert.NotNull(components);
            Assert.NotNull(components.Schemas);
            Assert.NotNull(components.Parameters);
            Assert.NotNull(components.Responses);
            Assert.Null(components.RequestBodies);
        }
    }
}
