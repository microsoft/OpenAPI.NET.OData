// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class NavigationPropertyPathItemHandlerTest
    {
        private NavigationPropertyPathItemHandler _pathItemHandler = new NavigationPropertyPathItemHandler();

        [Fact]
        public void CreatePathItemThrowsForNullContext()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("context",
                () => _pathItemHandler.CreatePathItem(context: null, path: new ODataPath()));
        }

        [Fact]
        public void CreatePathItemThrowsForNullPath()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("path",
                () => _pathItemHandler.CreatePathItem(new ODataContext(EdmCoreModel.Instance), path: null));
        }
        /*
        [Fact]
        public void CreatePathItemThrowsForNonNavigationPropertyPath()
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            var entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));
            Assert.Equal(ODataPathKind.Entity, path.Kind); // guard

            // Act
            Action test = () => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, "EntitySetPathItemHandler", path.Kind), exception.Message);
        }*/

        [Theory]
        [InlineData(true, true, new OperationType[] { OperationType.Get, OperationType.Patch })]
        [InlineData(true, false, new OperationType[] { OperationType.Get, OperationType.Post })]
        [InlineData(false, true, new OperationType[] { OperationType.Get })]
        [InlineData(false, false, new OperationType[] { OperationType.Get})]
        public void CreateCollectionNavigationPropertyPathItemReturnsCorrectPathItem(bool containment, bool keySegment, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("users");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => c.ContainsTarget == containment && c.TargetMultiplicity() == EdmMultiplicity.Many);
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            if (keySegment)
            {
                path.Push(new ODataKeySegment(property.ToEntityType()));
            }

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch })]
        [InlineData(false, new OperationType[] { OperationType.Get })]
        public void CreateSingleNavigationPropertyPathItemReturnsCorrectPathItem(bool containment, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("users");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => c.ContainsTarget == containment && c.TargetMultiplicity() != EdmMultiplicity.Many);
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
        }

        public static IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""DirectOrders"" Type=""Collection(NS.Order)"" />
      </EntityType>
      <EntityType Name=""Order"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name =""Default"">
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
         <EntitySet Name=""Orders"" EntityType=""NS.Order"" />
      </EntityContainer>
      <Annotations Target=""NS.Default/Customers"">
        {0}
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            IEdmModel model;
            IEnumerable<EdmError> errors;

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out errors);
            Assert.True(result);
            return model;
        }
    }
}
