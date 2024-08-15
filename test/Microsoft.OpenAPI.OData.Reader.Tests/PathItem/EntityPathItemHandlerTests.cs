// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
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
        private EntityPathItemHandler _pathItemHandler = new MyEntityPathItemHandler();

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
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete },
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
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete },
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
            Assert.Equal(new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete },
                pathItem.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Patch, OperationType.Delete })]
        public void CreateEntityPathItemWorksForReadByKeyRestrictionsCapablities(bool readable, OperationType[] expected)
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
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Patch, OperationType.Delete })]
        public void CreateEntityPathItemWorksForReadRestrictionsCapablities(bool readable, OperationType[] expected)
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
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Delete })]
        public void CreateEntityPathItemWorksForUpdateRestrictionsCapablities(bool updatable, OperationType[] expected)
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
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Patch })]
        public void CreateEntityPathItemWorksForDeleteRestrictionsCapablities(bool deletable, OperationType[] expected)
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
        [InlineData(false, new OperationType[] { OperationType.Get, OperationType.Patch, OperationType.Delete })]
        [InlineData(true, new OperationType[] { OperationType.Get, OperationType.Put, OperationType.Delete })]
        public void CreateEntityPathItemWorksForUpdateMethodRestrictionsCapabilities(bool updateMethod, OperationType[] expected)
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

        private void VerifyPathItemOperations(string annotation, OperationType[] expected)
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
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key));
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
            string isHiddenValue = (isHiddenExtension as OpenApiString)?.Value;
            Assert.Equal("true", isHiddenValue);

            pathItem.Extensions.TryGetValue("x-ms-isOwner", out IOpenApiExtension isOwnerExtension);
            string isOwnerValue = (isOwnerExtension as OpenApiString)?.Value;
            Assert.Equal("true", isOwnerValue);
        }
    }

    internal class MyEntityPathItemHandler : EntityPathItemHandler
    {
        protected override void AddOperation(OpenApiPathItem item, OperationType operationType)
        {
            item.AddOperation(operationType, new OpenApiOperation());
        }
    }
}
