// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class EdmModelExtensionsTests
    {
        [Fact]
        public void LoadAllNavigationSourcesReturnsCorrect()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");

            // Act
            var map = model.LoadAllNavigationSources();

            // Assert
            Assert.NotNull(map);
            Assert.Equal(3, map.Keys.Count);

            Assert.NotNull(map[person]);
            var navigationSource = map[person];
            Assert.Equal(new[] { "People", "NewComePeople", "Me" }, navigationSource.Select(n => n.Name));
        }

        [Fact]
        public void FindAllBaseTypesReturnsCorrect()
        {
            // Arrange
            IEdmEntityType baseEntityType = new EdmEntityType("NS", "Base");
            IEdmEntityType subEntityType = new EdmEntityType("NS", "Sub", baseEntityType);
            IEdmEntityType subSubEntityType = new EdmEntityType("NS", "SubSub", subEntityType);

            // 1. Act & Assert
            var baseTypes = baseEntityType.FindAllBaseTypes();
            Assert.Empty(baseTypes);

            // 2. Act & Assert
            baseTypes = subEntityType.FindAllBaseTypes();
            Assert.NotNull(baseTypes);
            var baseType = Assert.Single(baseTypes);
            Assert.Same(baseType, baseEntityType);

            // 3. Act & Assert
            baseTypes = subSubEntityType.FindAllBaseTypes();
            Assert.NotNull(baseTypes);
            Assert.Equal(2, baseTypes.Count());
            Assert.Equal(new[] { subEntityType, baseEntityType }, baseTypes);
        }

        [Fact]
        public void GetAllElementsReturnsElementsFromAllModels()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.InheritanceEdmModelAcrossReferences;

            // Act
            var elements = model.GetAllElements();

            // Assert
            Assert.Collection(elements, 
                e => Assert.Equal("Customer", e.Name),
                e => Assert.Equal("Default", e.Name),
                e => Assert.Equal("CustomerBase", e.Name));
        }
    }
}
