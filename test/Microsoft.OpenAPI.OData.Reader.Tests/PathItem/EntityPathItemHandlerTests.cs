// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class EntityPathItemHandlerTests
    {
        private EntityPathItemHandler _pathItemHandler = new MyEntityPathItemHandler(new());

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
        public void CreatePathItemThrowsForNonEntityPath()
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
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, _pathItemHandler.GetType().Name, path.Kind), exception.Message);
        }

        [Fact]
        public void CreateEntityPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(3, pathItem.Operations.Count);
            Assert.Equal(new HttpMethod[] { HttpMethod.Get, HttpMethod.Patch, HttpMethod.Delete },
                pathItem.Operations.Select(o => o.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityPathItemReturnsCorrectPathItemWithPathParameters(bool declarePathParametersOnPathItem)
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            OpenApiConvertSettings convertSettings = new OpenApiConvertSettings
            {
                DeclarePathParametersOnPathItem = declarePathParametersOnPathItem,
            };
            ODataContext context = new ODataContext(model, convertSettings);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(3, pathItem.Operations.Count);
            Assert.Equal(new HttpMethod[] { HttpMethod.Get, HttpMethod.Patch, HttpMethod.Delete },
                pathItem.Operations.Select(o => o.Key));

            if (declarePathParametersOnPathItem)
            {
                Assert.NotEmpty(pathItem.Parameters);
                Assert.Single(pathItem.Parameters);
            }
            else
            {
                Assert.Empty(pathItem.Parameters);
            }
        }

        [Fact]
        public void CreateEntityPathItemReturnsCorrectPathItemWithReferences()
        {
            // Test that references don't disturb the paths.

            // Arrange
            IEdmModel model = EdmModelHelper.InheritanceEdmModelAcrossReferences;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(3, pathItem.Operations.Count);
            Assert.Equal(new HttpMethod[] { HttpMethod.Get, HttpMethod.Patch, HttpMethod.Delete },
                pathItem.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, new string[] { "get", "patch", "delete" })]
        [InlineData(false, new string[] { "patch", "delete" })]
        public void CreateEntityPathItemWorksForReadByKeyRestrictionsCapablities(bool readable, string[] expected)
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
            VerifyPathItemOperations(annotation, expected);
        }

        [Theory]
        [InlineData(true, new string[] { "get", "patch", "delete" })]
        [InlineData(false, new string[] { "patch", "delete" })]
        public void CreateEntityPathItemWorksForReadRestrictionsCapablities(bool readable, string[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
    <PropertyValue Property=""Readable"" Bool=""{readable}"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperations(annotation, expected);
        }

        [Theory]
        [InlineData(true, new string[] { "get", "patch", "delete" })]
        [InlineData(false, new string[] { "get", "delete" })]
        public void CreateEntityPathItemWorksForUpdateRestrictionsCapablities(bool updatable, string[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""Updatable"" Bool=""{updatable}"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperations(annotation, expected);
        }

        [Theory]
        [InlineData(true, new string[] { "get", "patch", "delete" })]
        [InlineData(false, new string[] { "get", "patch" })]
        public void CreateEntityPathItemWorksForDeleteRestrictionsCapablities(bool deletable, string[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
  <Record>
    <PropertyValue Property=""Deletable"" Bool=""{deletable}"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperations(annotation, expected);
        }

        [Theory]
        [InlineData(false, new string[] { "get", "patch", "delete" })]
        [InlineData(true, new string[] { "get", "put", "delete" })]
        public void CreateEntityPathItemWorksForUpdateMethodRestrictionsCapabilities(bool updateMethod, string[] expected)
        {
            // Arrange
            string annotation = updateMethod ? $@"
<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
  <Record>
    <PropertyValue Property=""UpdateMethod"">
      <EnumMember>Org.OData.Capabilities.V1.HttpMethod/PUT</EnumMember>
    </PropertyValue>
  </Record>
</Annotation>" : "";

            // Assert
            VerifyPathItemOperations(annotation, expected);
        }

        private void VerifyPathItemOperations(string annotation, string[] expected)
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key.ToString().ToLowerInvariant()));
        }

        [Fact]
        public void CreateEntityPathItemAddsCustomAttributeValuesToPathExtensions()
        {
            // Arrange
            IEdmModel model = EntitySetPathItemHandlerTests.GetEdmModel(annotation: "");
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping = new()
            {
                {
                    "ags:IsHidden", "x-ms-isHidden"
                },
                {
                    "isOwner", "x-ms-isOwner"
                }
            };
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Extensions);

            pathItem.Extensions.TryGetValue("x-ms-isHidden", out IOpenApiExtension isHiddenExtension);
            string isHiddenValue = (isHiddenExtension as OpenApiAny).Node.GetValue<string>();
            Assert.Equal("true", isHiddenValue);

            pathItem.Extensions.TryGetValue("x-ms-isOwner", out IOpenApiExtension isOwnerExtension);
            string isOwnerValue = (isOwnerExtension as OpenApiAny).Node.GetValue<string>();
            Assert.Equal("true", isOwnerValue);
        }
    }

    internal class MyEntityPathItemHandler : EntityPathItemHandler
    {
        public MyEntityPathItemHandler(OpenApiDocument document) : base(document)
        {
          
        }
        protected override void AddOperation(OpenApiPathItem item, HttpMethod operationType)
        {
            item.AddOperation(operationType, new OpenApiOperation());
        }
    }
}
