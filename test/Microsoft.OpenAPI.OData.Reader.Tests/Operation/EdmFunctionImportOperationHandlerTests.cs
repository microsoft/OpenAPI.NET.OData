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
    public class EdmFunctionImportOperationHandlerTests
    {
        private EdmFunctionImportOperationHandler _operationHandler = new EdmFunctionImportOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionImportReturnsCorrectOperation(bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            var settings = new OpenApiConvertSettings
            {
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            var functionImport = model.EntityContainer.FindOperationImports("GetPersonWithMostFriends").FirstOrDefault();
            Assert.NotNull(functionImport);
            ODataPath path = new ODataPath(new ODataOperationImportSegment(functionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke functionImport GetPersonWithMostFriends", operation.Summary);
            Assert.Equal("The person with most friends.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Empty(operation.Parameters);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionImportReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), false, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport = new EdmFunctionImport(container, "MyFunction", function);
            container.AddElement(functionImport);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataOperationImportSegment(functionImport));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("FunctionImport.MyFunction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationIdWithSHA5ForOverloadEdmFunctionImport(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function1 = new EdmFunction("NS", "MyFunction1", EdmCoreModel.Instance.GetString(false), false, null, false);
            function1.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function1.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function1);

            EdmFunction function2 = new EdmFunction("NS", "MyFunction1", EdmCoreModel.Instance.GetString(false), false, null, false);
            function2.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function2.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            function2.AddParameter("otherParam", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function2);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmFunctionImport functionImport1 = new EdmFunctionImport(container, "MyFunction", function1);
            EdmFunctionImport functionImport2 = new EdmFunctionImport(container, "MyFunction", function2);
            container.AddElement(functionImport1);
            container.AddElement(functionImport2);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                AddSingleQuotesForStringParameters = true,
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataOperationImportSegment(functionImport1));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("FunctionImport.MyFunction-cc1c", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OperationRestrictionsTermWorksToCreateOperationForEdmFunctionImport(bool enableAnnotation)
        {
            string template = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Function Name=""GetNearestAirport"">
        <Parameter Name=""lat"" Type=""Edm.Double"" Nullable=""false"" />
        <Parameter Name=""lon"" Type=""Edm.Double"" Nullable=""false"" />
        <ReturnType Type=""Edm.String"" />
      </Function>
      <EntityContainer Name=""GraphService"">
        <FunctionImport Name=""GetNearestAirport"" Function=""NS.GetNearestAirport"" >
         {0}
        </FunctionImport>
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>
";

            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.OperationRestrictions"">
  <Record>
    <PropertyValue Property=""CustomHeaders"">
      <Collection>
        <Record>
          <PropertyValue Property=""Name"" String=""myhead1"" />
          <PropertyValue Property=""Required"" Bool=""true"" />
        </Record>
        <Record>
          <PropertyValue Property=""Name"" String=""myhead2"" />
          <PropertyValue Property = ""Description"" String = ""This is the description for myhead2."" />
          <PropertyValue Property = ""Required"" Bool = ""false"" />
        </Record>
        <Record>
          <PropertyValue Property=""Name"" String=""myhead3"" />
          <PropertyValue Property = ""DocumentationURL"" String = ""https://foo.bar.com/myhead3"" />
          <PropertyValue Property = ""Required"" Bool = ""false"" />
        </Record>
        <Record>
          <PropertyValue Property=""Name"" String=""myhead4"" />
          <PropertyValue Property = ""Description"" String = ""This is the description for myhead4."" />
          <PropertyValue Property = ""DocumentationURL"" String = ""https://foo.bar.com/myhead4"" />
          <PropertyValue Property = ""Required"" Bool = ""false"" />
          <PropertyValue Property = ""ExampleValues"" >
            <Collection>
              <Record>
                 <PropertyValue Property = ""Value"" String = ""sample"" />
                 <PropertyValue Property = ""Description"" String = ""The sample description."" />
                 </Record>
            </Collection>
          </PropertyValue>
        </Record>
       </Collection>
    </PropertyValue>
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
  </Record>
</Annotation>";

            // Arrange
            string csdl = string.Format(template, enableAnnotation ? annotation : "");

            var edmModel = CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            Assert.NotNull(edmModel);
            IEdmOperationImport operationImport = edmModel.EntityContainer.FindOperationImports("GetNearestAirport").FirstOrDefault();
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

                Assert.Contains(@"
    {
      ""name"": ""myhead1"",
      ""in"": ""header"",
      ""required"": true,
      ""schema"": {
        ""type"": ""string""
      }
    }
".ChangeLineBreaks(), json);

                // Assert with no DocumentationURL value
                Assert.Contains(@"
    {
      ""name"": ""myhead2"",
      ""in"": ""header"",
      ""description"": ""This is the description for myhead2."",
      ""schema"": {
        ""type"": ""string""
      }
    }
".ChangeLineBreaks(), json);

                // Assert with no Description value
                Assert.Contains(@"
    {
      ""name"": ""myhead3"",
      ""in"": ""header"",
      ""description"": ""Documentation URL: https://foo.bar.com/myhead3"",
      ""schema"": {
        ""type"": ""string""
      }
    }
".ChangeLineBreaks(), json);

                // Assert with both DocumentationURL and Description values
                Assert.Contains(@"
    {
      ""name"": ""myhead4"",
      ""in"": ""header"",
      ""description"": ""This is the description for myhead4. Documentation URL: https://foo.bar.com/myhead4"",
      ""schema"": {
        ""type"": ""string""
      },
      ""examples"": {
        ""example-1"": {
          ""description"": ""The sample description."",
          ""value"": ""sample""
        }
      }
    }
".ChangeLineBreaks(), json);

            }
            else
            {
                Assert.Empty(operation.Security);
            }
        }
    }
}