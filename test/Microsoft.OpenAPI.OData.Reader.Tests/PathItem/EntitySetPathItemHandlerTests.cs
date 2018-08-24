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
    public class EntitySetPathItemHandlerTests
    {
        private EntitySetPathItemHandler _pathItemHandler = new EntitySetPathItemHandler();

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
        public void CreatePathItemThrowsForNonEntitySetPath()
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
        }

        [Fact]
        public void CreateEntitySetPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(2, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Post },
                pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [InlineData("None")]
        [InlineData("Single")]
        [InlineData("Recursive")]
        public void CreateEntitySetPathItemWorksForNavigationRestrictionsCapablities(string navigationType)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
    <PropertyValue Property=""Navigability"">
       <EnumMember>Org.OData.Capabilities.V1.NavigationType/{0}</EnumMember>
    </PropertyValue>
  </Record>
</Annotation>", navigationType);

            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Contains(OperationType.Post, pathItem.Operations.Select(e => e.Key));

            if (navigationType == "None")
            {
                Assert.DoesNotContain(OperationType.Get, pathItem.Operations.Select(e => e.Key));
            }
            else
            {
                Assert.Contains(OperationType.Get, pathItem.Operations.Select(e => e.Key));
            }
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Post })]
        [InlineData(false, new OperationType[] { OperationType.Get })]
        public void CreateEntitySetPathItemWorksForInsertRestrictionsCapablities(bool insertable, OperationType[] expected)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
  <Record>
    <PropertyValue Property=""Insertable"" Bool=""{0}"" />
  </Record>
</Annotation>", insertable);

            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
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
      </EntityType>
      <EntityContainer Name =""Default"">
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
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
