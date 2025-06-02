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
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class EntitySetPathItemHandlerTests
    {
        private readonly EntitySetPathItemHandler _pathItemHandler = new MyEntitySetPathItemHandler(new());

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
            var path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));
            Assert.Equal(ODataPathKind.Entity, path.Kind); // guard

            // Act
            Action test = () => _pathItemHandler.CreatePathItem(context, path);

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Equal(String.Format(SRResource.InvalidPathKindForPathItemHandler, _pathItemHandler.GetType().Name, path.Kind), exception.Message);
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
            Assert.Equal([HttpMethod.Get, HttpMethod.Post],
                pathItem.Operations.Select(o => o.Key));
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, new [] { "get", "post" })]
        [InlineData(false, new [] { "post" })]
        public void CreateEntitySetPathItemWorksForReadRestrictionsCapabilities(bool readable, string[] expected)
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
        [InlineData(true, new [] { "get", "post" })]
        [InlineData(false, new [] { "get" })]
        public void CreateEntitySetPathItemWorksForInsertRestrictionsCapablities(bool insertable, string[] expected)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
  <Record>
    <PropertyValue Property=""Insertable"" Bool=""{insertable}"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperations(annotation, expected);
        }

        [Fact]
        public void CreateEntitySetPathItemWorksForReadAndInsertRestrictionsCapablities()
        {
            // Arrange
            string annotation = @"
<Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
  <Record>
    <PropertyValue Property=""Insertable"" Bool=""false"" />
  </Record>
</Annotation>
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
    <PropertyValue Property=""Readable"" Bool=""false"" />
  </Record>
</Annotation>";

            // Assert
            VerifyPathItemOperations(annotation, []);
        }

        private void VerifyPathItemOperations(string annotation, string[] expected)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);

            if (expected is {Length: > 0})
            {
              Assert.NotNull(pathItem.Operations);
              Assert.Equal(expected, pathItem.Operations.Select(e => e.Key.ToString().ToLowerInvariant()));
            }
            else
              Assert.Null(pathItem.Operations);
        }

        [Fact]
        public void CreateEntitySetPathItemAddsCustomAttributeValuesToPathExtensions()
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation: "");
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping.Add("ags:IsHidden", "x-ms-isHidden");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            ODataPath path = new(new ODataNavigationSourceSegment(entitySet));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Extensions);

            pathItem.Extensions.TryGetValue("x-ms-isHidden", out var value);
            string isHiddenValue = Assert.IsType<JsonNodeExtension>(value).Node.GetValue<string>();
            Assert.Equal("true", isHiddenValue);
        }

        public static IEdmModel GetEdmModel(string annotation, string target = "\"NS.Default/Customers\"", string customXMLAttribute = null)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"" xmlns:ags=""http://aggregator.microsoft.com/internal"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <ComplexType Name=""Address"">
        <Property Name=""City"" Type=""Edm.String"" />
      </ComplexType>
      <EntityType Name=""Customer"" ags:IsOwner=""true"" ags:IsHidden=""true"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""BillingAddress"" Type=""NS.Address"" />
        <Property Name=""MailingAddress"" Type=""NS.Address"" Nullable=""false"" />
        <Property Name=""AlternativeAddresses"" Type=""Collection(NS.Address)"" Nullable=""false"" ags:IsHidden=""true""/>
      </EntityType>
      <EntityContainer Name =""Default"">
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" ags:IsHidden=""true""/>
      </EntityContainer>
      <Annotations Target={0}>
        {1}
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, target, annotation);

            IEdmModel model;
            IEnumerable<EdmError> errors;

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out errors);
            Assert.True(result);
            return model;
        }
    }

    internal class MyEntitySetPathItemHandler(OpenApiDocument document) : EntitySetPathItemHandler(document)
    {
        protected override void AddOperation(OpenApiPathItem item, HttpMethod operationType)
        {
            item.AddOperation(operationType, new OpenApiOperation());
        }
    }
}
