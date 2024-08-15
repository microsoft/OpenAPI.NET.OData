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
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(entitySet); // guard
            Assert.NotNull(singleton);
            IEdmEntityType entityType = entitySet.EntityType;

            IEdmStructuralProperty sp = entityType.DeclaredStructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataStreamPropertySegment(sp.Name));

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.DeclaredNavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(singleton),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);
            var pathItem2 = _pathItemHandler.CreatePathItem(context, path2);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem2);

            Assert.NotNull(pathItem.Operations);
            Assert.NotNull(pathItem2.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.NotEmpty(pathItem2.Operations);
            Assert.Equal(3, pathItem.Operations.Count);
            Assert.Equal(3, pathItem2.Operations.Count);
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete },
                pathItem.Operations.Select(o => o.Key));
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete },
                pathItem2.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Put, OperationType.Delete })]
        public void CreateMediaEntityPathItemWorksForReadByKeyRestrictionsCapabilities(bool readable, OperationType[] expected)
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
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Delete })]
        public void CreateMediaEntityPathItemWorksForUpdateRestrictionsCapabilities(bool updatable, OperationType[] expected)
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

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Delete })]
        public void CreateMediaEntityPathItemWorksForUpdateRestrictionsCapabilitiesWithTargetPathAnnotations(bool updatable, OperationType[] expected)
        {
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
    <Record>
    <PropertyValue Property=""Updatable"" Bool=""{!updatable}"" />
    </Record>
</Annotation>";

            // Arrange
            string streamPropertyTargetPathAnnotation = $@"
<Annotations Target=""microsoft.graph.GraphService/Todos/Logo"">
    <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
      <Record>
        <PropertyValue Property=""Updatable"" Bool=""{updatable}"" />
      </Record>
    </Annotation>
</Annotations>";

            // Assert
            VerifyPathItemOperationsForStreamPropertySegment(annotation, expected, streamPropertyTargetPathAnnotation);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Put })]
        public void CreateMediaEntityPathItemWorksForDeleteRestrictionsCapabilities(bool deletable, OperationType[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
  <Record>
    <PropertyValue Property=""Deletable"" Bool=""{deletable}"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperationsForStreamPropertySegment(annotation, expected);
            VerifyPathItemOperationsForStreamContentSegment(annotation, expected);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Put })]
        public void CreateMediaEntityPathItemWorksForDeleteRestrictionsCapabilitiesWithTargetPathAnnotations(bool deletable, OperationType[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
  <Record>
    <PropertyValue Property=""Deletable"" Bool=""{!deletable}"" />
  </Record>
</Annotation>";


            // Arrange
            string streamPropertyTargetPathAnnotation = $@"
<Annotations Target=""microsoft.graph.GraphService/Todos/Logo"">
    <Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
      <Record>
        <PropertyValue Property=""Deletable"" Bool=""{deletable}"" />
      </Record>
    </Annotation>
</Annotations>";

            // Assert
            VerifyPathItemOperationsForStreamPropertySegment(annotation, expected, streamPropertyTargetPathAnnotation);
        }

        private void VerifyPathItemOperationsForStreamPropertySegment(string annotation, OperationType[] expected, string targetPathAnnotations = "")
        {
            // Arrange
            
            IEdmModel model = GetEdmModel(annotation, targetPathAnnotations);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Todos");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType;

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

        private void VerifyPathItemOperationsForStreamContentSegment(string annotation, OperationType[] expected, string targetPathAnnotations = null)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation, targetPathAnnotations);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(entitySet); // guard
            Assert.NotNull(singleton);
            IEdmEntityType entityType = entitySet.EntityType;

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.DeclaredNavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(singleton),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataStreamContentSegment());

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);
            var pathItem2 = _pathItemHandler.CreatePathItem(context, path2);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem2);

            Assert.NotNull(pathItem.Operations);
            Assert.NotNull(pathItem2.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.NotEmpty(pathItem2.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
            Assert.Equal(expected, pathItem2.Operations.Select(e => e.Key));
        }

        private IEdmModel GetEdmModel(string annotation, string targetPathAnnotation = "")
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
      <EntityType Name=""user"" OpenType=""true"">
        <NavigationProperty Name = ""photo"" Type = ""microsoft.graph.profilePhoto"" ContainsTarget = ""true"" />
      </EntityType>
      <EntityType Name=""profilePhoto"" HasStream=""true"">
        <Property Name = ""height"" Type = ""Edm.Int32"" />
        <Property Name = ""width"" Type = ""Edm.Int32"" />
      </EntityType >
      <EntityContainer Name =""GraphService"">
        <EntitySet Name=""Todos"" EntityType=""microsoft.graph.Todo"" />
        <Singleton Name=""me"" Type=""microsoft.graph.user"" />
      </EntityContainer>
      <Annotations Target=""microsoft.graph.GraphService/Todos"" >
       {0}
      </Annotations>
      <Annotations Target=""microsoft.graph.GraphService/me"" >
       {0}
      </Annotations>
      {1}
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation, targetPathAnnotation);
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
