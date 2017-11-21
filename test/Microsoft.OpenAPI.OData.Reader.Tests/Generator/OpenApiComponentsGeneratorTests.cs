// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;
using Microsoft.OpenApi.OData.Tests;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiComponentsGeneratorTest
    {
        private OpenApiConvertSettings _settings = new OpenApiConvertSettings();

        [Fact]
        public void CtorThrowArgumentNullModel()
        {
            // Arrange
            IEdmModel model = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("model", () => new OpenApiComponentsGenerator(model, null));
        }

        [Fact]
        public void CtorThrowArgumentNullSetting()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("settings", () => new OpenApiComponentsGenerator(model, null));
        }

        [Fact]
        public void CreateComponentsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            OpenApiComponentsGenerator generator = new OpenApiComponentsGenerator(model, _settings);

            // Act
            var components = generator.CreateComponents();

            // Assert
            Assert.NotNull(components);
            Assert.NotNull(components.Schemas);
            Assert.NotNull(components.Parameters);
            Assert.NotNull(components.Responses);
            Assert.Null(components.RequestBodies);
        }
    }
}
