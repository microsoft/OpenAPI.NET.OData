// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Properties;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class OperationImportPathItemHandlerTest
    {
        private OperationImportPathItemHandler _pathItemHandler = new MyOperationImportPathItemHandler();

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
        public void CreatePathItemThrowsForNonOperationImportPath()
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

        [Theory]
        [InlineData("GetNearestAirport", OperationType.Get)]
        [InlineData("ResetDataSource", OperationType.Post)]
        public void CreatePathItemForOperationImportReturnsCorrectPathItem(string operationImport,
            OperationType operationType)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmOperationImport edmOperationImport = model.EntityContainer
                .OperationImports().FirstOrDefault(o => o.Name == operationImport);
            Assert.NotNull(edmOperationImport); // guard
            ODataPath path = new ODataPath(new ODataOperationImportSegment(edmOperationImport));

            // Act
            OpenApiPathItem pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);
            var operationKeyValue = Assert.Single(pathItem.Operations);
            Assert.Equal(operationType, operationKeyValue.Key);
            Assert.NotNull(operationKeyValue.Value);
            Assert.NotEmpty(pathItem.Description);
        }

        [Theory]
        [InlineData(true, "GetNearestCustomers", OperationType.Get)]
        [InlineData(false, "GetNearestCustomers", null)]
        [InlineData(true, "ResetDataSource", OperationType.Post)]
        [InlineData(false, "ResetDataSource", OperationType.Post)]
        public void CreatePathItemForOperationImportWithReadRestrictionsReturnsCorrectPathItem(bool readable, string operationImport,
            OperationType? operationType)
        {
            // Arrange
            string annotation = $@"
<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
    <PropertyValue Property=""Readable"" Bool=""{readable}"" />
  </Record>
</Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            ODataContext context = new ODataContext(model);
            IEdmOperationImport edmOperationImport = model.EntityContainer
                .OperationImports().FirstOrDefault(o => o.Name == operationImport);
            Assert.NotNull(edmOperationImport); // guard
            ODataPath path = new ODataPath(new ODataOperationImportSegment(edmOperationImport));

            // Act
            OpenApiPathItem pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Operations);
            if (operationType == null)
            {
                Assert.Empty(pathItem.Operations);
            }
            else
            {
                var operationKeyValue = Assert.Single(pathItem.Operations);
                Assert.Equal(operationType, operationKeyValue.Key);
                Assert.NotNull(operationKeyValue.Value);
            }
        }

        [Theory]
        [InlineData("GetNearestAirport")]
        [InlineData("ResetDataSource")]
        public void CreateOperationImportPathItemAddsCustomAttributeValuesToPathExtensions(string operationImport)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping.Add("ags:IsHidden", "x-ms-isHidden");
            IEdmOperationImport edmOperationImport = model.EntityContainer
                .OperationImports().FirstOrDefault(o => o.Name == operationImport);
            Assert.NotNull(edmOperationImport); // guard
            ODataPath path = new(new ODataOperationImportSegment(edmOperationImport));

            // Act
            OpenApiPathItem pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Extensions);

            pathItem.Extensions.TryGetValue("x-ms-isHidden", out IOpenApiExtension isHiddenExtension);
            string isHiddenValue = (isHiddenExtension as OpenApiString)?.Value;
            Assert.Equal("true", isHiddenValue);
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
      <Action Name=""ResetDataSource"" />
      <Function Name=""GetNearestCustomers"" >
        <Parameter Name=""name"" Type=""Edm.String"" Nullable=""false"" />
        <ReturnType Type=""NS.Customer"" />
      </Function>
       <EntityContainer Name =""Default"">
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
         <FunctionImport Name=""GetNearestCustomers"" Function=""NS.GetNearestCustomer"" EntitySet =""Customers"" >
           {0}
         </FunctionImport>
         <ActionImport Name=""ResetDataSource"" Action=""NS.ResetDataSource"" >
          {0}
         </ActionImport>
       </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            IEdmModel model;
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out _);
            Assert.True(result);
            return model;
        }
    }

    internal class MyOperationImportPathItemHandler : OperationImportPathItemHandler
    {
        protected override void AddOperation(OpenApiPathItem item, OperationType operationType)
        {
            item.AddOperation(operationType, new OpenApiOperation());
        }
    }
}
