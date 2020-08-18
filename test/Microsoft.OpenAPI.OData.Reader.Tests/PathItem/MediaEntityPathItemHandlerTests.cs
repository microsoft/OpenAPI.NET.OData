// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using System;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class MediaEntityPathItemHandlerTests
    {
        private readonly MediaEntityPathItemHandler _pathItemHandler = new MyMediaEntityPathItemHandler();

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
        public void CreatePathItemThrowsForNonMediaEntityPath()
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            ODataContext context = new ODataContext(model);
            var entitySet = model.EntityContainer.FindEntitySet("Todos");
            Assert.NotNull(entitySet); // guard
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            void test() => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(string.Format(SRResource.InvalidPathKindForPathItemHandler, _pathItemHandler.GetType().Name, path.Kind), exception.Message);
        }

        [Fact]
        public void CreateMediaEntityPathItemReturnsCorrectItem()
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            ODataContext context = new ODataContext(model);
            var entitySet = model.EntityContainer.FindEntitySet("Todos");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmStructuralProperty sp = entityType.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataStreamPropertySegment(sp.Name));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(2, pathItem.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Put },
                pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put })]
        [InlineData(false, new OperationType[] { OperationType.Put })]
        public void CreateMediaEntityPathItemWorksForReadByKeyRestrictionsCapablities(bool readable, OperationType[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
    <PropertyValue Property=""ReadByKeyRestrictions"" >
      <Record>
        <PropertyValue Property=""Readable"" Bool=""{readable}"" />
      </Record>
    </PropertyValue>
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperationsForStreamPropertySegment(annotation, expected);
            VerifyPathItemOperationsForStreamContentSegment(annotation, expected);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put })]
        [InlineData(false, new OperationType[] { OperationType.Get })]
        public void CreateMediaEntityPathItemWorksForUpdateRestrictionsCapablities(bool updatable, OperationType[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""Updatable"" Bool=""{updatable}"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperationsForStreamPropertySegment(annotation, expected);
            VerifyPathItemOperationsForStreamContentSegment(annotation, expected);
        }

        private void VerifyPathItemOperationsForStreamPropertySegment(string annotation, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Todos");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataStreamContentSegment());

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
        }

        private void VerifyPathItemOperationsForStreamContentSegment(string annotation, OperationType[] expected)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Todos");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmStructuralProperty sp = entityType.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataStreamPropertySegment(sp.Name));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""microsoft.graph"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Todo"" HasStream=""true"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Logo"" Type=""Edm.Stream""/>
        <Property Name = ""Description"" Type = ""Edm.String"" />
         </EntityType>
      <EntityContainer Name =""TodoService"">
         <EntitySet Name=""Todos"" EntityType=""microsoft.graph.Todo"" />
      </EntityContainer>
      <Annotations Target=""microsoft.graph.TodoService/Todos"">
        {0}
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }
    }

    internal class MyMediaEntityPathItemHandler : MediaEntityPathItemHandler
    {
        protected override void AddOperation(OpenApiPathItem item, OperationType operationType)
        {
            item.AddOperation(operationType, new OpenApiOperation());
        }
    }
}
