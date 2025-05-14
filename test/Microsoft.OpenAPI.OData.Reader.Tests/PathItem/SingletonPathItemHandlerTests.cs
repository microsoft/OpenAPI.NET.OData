// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class SingletonPathItemHandlerTest
    {
        private SingletonPathItemHandler _pathItemHandler = new MySingletonPathItemHandler(new());

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
        public void CreatePathItemThrowsForNonSingletonPath()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            var entitySet = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(entitySet); // guard
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            Action test = () => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, _pathItemHandler.GetType().Name, path.Kind), exception.Message);
        }

        [Fact]
        public void CreateSingletonPathItemReturnsCorrectPathItem()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(singleton); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(2, pathItem.Operations.Count);
            Assert.Equal([HttpMethod.Get, HttpMethod.Patch],
                pathItem.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, new string[] { "get", "patch" })]
        [InlineData(false, new string[] { "patch" })]
        public void CreateSingletonPathItemWorksForReadRestrictionsCapablities(bool readable, string[] expected)
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
        [InlineData(true, new string[] { "get", "patch" })]
        [InlineData(false, new string[] { "get" })]
        public void CreateSingletonPathItemWorksForUpdateRestrictionsCapablities(bool updatable, string[] expected)
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

        private void VerifyPathItemOperations(string annotation, string[] expected)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(singleton); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            Assert.NotNull(pathItem.Operations);
            Assert.NotEmpty(pathItem.Operations);
            Assert.Equal(expected, pathItem.Operations.Select(e => e.Key.ToString().ToLowerInvariant()));
        }

        [Fact]
        public void CreateSingletonPathItemAddsCustomAttributeValuesToPathExtensions()
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation: "");
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping.Add("IsHidden", "x-ms-isHidden");
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(singleton); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Extensions);

            pathItem.Extensions.TryGetValue("x-ms-isHidden", out var value);
            string isHiddenValue = Assert.IsType<JsonNodeExtension>(value).Node.GetValue<string>();
            Assert.Equal("true", isHiddenValue);
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"" xmlns:ags=""http://aggregator.microsoft.com/internal"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name =""Default"">
         <Singleton Name=""Me"" Type=""NS.Customer"" ags:IsHidden=""true""/>
      </EntityContainer>
      <Annotations Target=""NS.Default/Me"">
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

    internal class MySingletonPathItemHandler : SingletonPathItemHandler
    {
        public MySingletonPathItemHandler(OpenApiDocument document) : base(document)
        {
          
        }
        protected override void AddOperation(OpenApiPathItem item, HttpMethod operationType)
        {
            item.AddOperation(operationType, new OpenApiOperation());
        }
    }
}
