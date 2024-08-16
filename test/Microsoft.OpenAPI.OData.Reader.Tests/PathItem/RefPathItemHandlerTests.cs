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
using Microsoft.OpenApi.OData.Properties;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class RefPathItemHandlerTest
    {
        private RefPathItemHandler _pathItemHandler = new RefPathItemHandler();

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

        [Fact]
        public void CreatePathItemThrowsForNonNavigationPropertyPath()
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet));
            Assert.Equal(ODataPathKind.EntitySet, path.Kind); // guard

            // Act
            Action test = () => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, "RefPathItemHandler", path.Kind), exception.Message);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Delete}, true)]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Post, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        public void CreateNavigationPropertyRefPathItemReturnsCorrectPathItem(bool collectionNav, OperationType[] expected, bool indexedColNav = false)
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType;

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties()
                .FirstOrDefault(c => !c.ContainsTarget &&
                (collectionNav ? c.TargetMultiplicity() == EdmMultiplicity.Many : c.TargetMultiplicity() != EdmMultiplicity.Many));
            Assert.NotNull(property);

            ODataPath path;
            if (collectionNav && indexedColNav)
            {
                // DELETE ~/entityset/{key}/collection-valued-Nav/{key}/$ref
                path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property),
                new ODataKeySegment(property.ToEntityType()),
                ODataRefSegment.Instance);
            }
            else
            {
                path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property),
                ODataRefSegment.Instance);
            }

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
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
        <NavigationProperty Name=""ContainedOrders"" Type=""Collection(NS.Order)"" ContainsTarget=""true"" />
        <NavigationProperty Name=""Orders"" Type=""Collection(NS.Order)"" />
        <NavigationProperty Name=""ContainedMyOrder"" Type=""NS.Order"" Nullable=""false"" ContainsTarget=""true"" />
        <NavigationProperty Name=""MyOrder"" Type=""NS.Order"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""Order"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""ContainedOrderLines"" Type=""Collection(NS.OrderLine)"" ContainsTarget=""true"" />
        <NavigationProperty Name=""OrderLines"" Type=""Collection(NS.OrderLine)"" />
        <NavigationProperty Name=""ContainedMyOrderLine"" Type=""NS.OrderLine"" Nullable=""false"" ContainsTarget=""true"" />
        <NavigationProperty Name=""MyOrderLine"" Type=""NS.OrderLine"" Nullable=""false"" />
      </EntityType>
      <EntityType Name=""OrderLine"">
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
