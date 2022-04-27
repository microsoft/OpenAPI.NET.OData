// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EdmActionImportOperationHandlerTests
    {
        private EdmActionImportOperationHandler _operationHandler = new EdmActionImportOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmActionImportReturnsCorrectOperation(bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            var settings = new OpenApiConvertSettings
            {
                UseHTTPStatusCodeClass2XX = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);

            var actionImport = model.EntityContainer.FindOperationImports("ResetDataSource").FirstOrDefault();
            Assert.NotNull(actionImport);
            ODataPath path = new ODataPath(new ODataOperationImportSegment(actionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke actionImport ResetDataSource", operation.Summary);
            Assert.Equal("Resets the data source to default values.", operation.Description);
            Assert.NotNull(operation.Tags);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmActionImportReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmAction action = new EdmAction("NS", "MyAction", EdmCoreModel.Instance.GetString(false), false, null);
            action.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            EdmActionImport actionImport = new EdmActionImport(container, "MyAction", action);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataOperationImportSegment(actionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("ActionImport.MyAction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OperationRestrictionsTermWorksToCreateOperationForEdmActionImport(bool enableAnnotation)
        {
            string template = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Action Name=""ResetDataSource"" />
      <EntityContainer Name=""GraphService"">
        <ActionImport Name=""ResetDataSource"" Action=""NS.ResetDataSource"" >
         {0}
        </ActionImport>
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
";

            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.OperationRestrictions"">
  <Record>
    <PropertyValue Property=""Permissions"">
      <Collection>
        <Record>
          <PropertyValue Property=""SchemeName"" String=""Delegated (work or school account)"" />
          <PropertyValue Property=""Scopes"">
            <Collection>
              <Record>
                <PropertyValue Property=""Scope"" String=""User.ReadBasic.All"" />
              </Record>
              <Record>
                <PropertyValue Property=""Scope"" String=""User.Read.All"" />
              </Record>
              <Record>
                <PropertyValue Property=""Scope"" String=""Directory.Read.All"" />
              </Record>
            </Collection>
          </PropertyValue>
        </Record>
        <Record>
          <PropertyValue Property=""SchemeName"" String=""Application"" />
          <PropertyValue Property=""Scopes"">
            <Collection>
              <Record>
                <PropertyValue Property=""Scope"" String=""User.Read.All"" />
              </Record>
              <Record>
                <PropertyValue Property=""Scope"" String=""Directory.Read.All"" />
              </Record>
            </Collection>
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
    <PropertyValue Property=""CustomQueryOptions"">
      <Collection>
        <Record>
          <PropertyValue Property=""Name"" String=""odata-debug"" />
          <PropertyValue Property=""Description"" String=""Debug support for OData services"" />
          <PropertyValue Property=""Required"" Bool=""false"" />
          <PropertyValue Property=""ExampleValues"">
            <Collection>
              <Record>
                <PropertyValue Property=""Value"" String=""html"" />
                <PropertyValue Property=""Description"" String=""Service responds with self-contained..."" />
              </Record>
              <Record>
                <PropertyValue Property=""Value"" String=""json"" />
                <PropertyValue Property=""Description"" String=""Service responds with JSON document..."" />
              </Record>
            </Collection>
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>";

            // Arrange
            string csdl = string.Format(template, enableAnnotation ? annotation : "");

            var edmModel = CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            Assert.NotNull(edmModel);
            IEdmOperationImport operationImport = edmModel.EntityContainer.FindOperationImports("ResetDataSource").FirstOrDefault();
            Assert.NotNull(operationImport);

            ODataContext context = new ODataContext(edmModel);

            ODataPath path = new ODataPath(new ODataOperationImportSegment(operationImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.NotNull(operation.Security);

            if (enableAnnotation)
            {
                Assert.Equal(2, operation.Security.Count);

                string json = operation.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                Assert.Contains(@"
  ""security"": [
    {
      ""Delegated (work or school account)"": [
        ""User.ReadBasic.All"",
        ""User.Read.All"",
        ""Directory.Read.All""
      ]
    },
    {
      ""Application"": [
        ""User.Read.All"",
        ""Directory.Read.All""
      ]
    }
  ],".ChangeLineBreaks(), json);

                // with custom query options
                Assert.Contains(@"
  ""parameters"": [
    {
      ""name"": ""odata-debug"",
      ""in"": ""query"",
      ""description"": ""Debug support for OData services"",
      ""schema"": {
        ""type"": ""string""
      },
      ""examples"": {
        ""example-1"": {
          ""description"": ""Service responds with self-contained..."",
          ""value"": ""html""
        },
        ""example-2"": {
          ""description"": ""Service responds with JSON document..."",
          ""value"": ""json""
        }
      }
    }
  ],".ChangeLineBreaks(), json);

            }
            else
            {
                Assert.Empty(operation.Security);
            }
        }
    }
}
