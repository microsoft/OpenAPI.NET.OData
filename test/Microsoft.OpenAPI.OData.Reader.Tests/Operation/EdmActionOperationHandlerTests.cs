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
    public class EdmActionOperationHandlerTests
    {
        private EdmActionOperationHandler _operationHandler = new EdmActionOperationHandler();

        [Fact]
        public void CreateOperationForEdmActionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new ODataContext(model);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmAction action = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == "ShareTrip");
            Assert.NotNull(action);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType()), new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke action ShareTrip", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Actions", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(new string[] { "UserName" }, operation.Parameters.Select(p => p.Name));

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("Action parameters", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Fact]
        public void CreateOperationForEdmActionReturnsCorrectOperationHierarhicalClass()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.ContractServiceModel;
            ODataContext context = new ODataContext(model);
            const string entitySetName = "Accounts";
            const string actionName = "AttachmentsAdd";
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(entitySetName);
            Assert.NotNull(entitySet);

            IEdmAction action = model.SchemaElements.OfType<IEdmAction>().First(f => f.Name == actionName);
            Assert.NotNull(action);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()), new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal($"Invoke action {actionName}", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal($"{entitySetName}.Actions", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);
            Assert.Equal(new string[] { "id" }, operation.Parameters.Select(p => p.Name));

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("Action parameters", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmActionReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmAction action = new EdmAction("NS", "MyAction", EdmCoreModel.Instance.GetString(false), true, null);
            action.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            action.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.MyAction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmActionWithTypeCastReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);
            EdmAction action = new EdmAction("NS", "MyAction", EdmCoreModel.Instance.GetString(false), true, null);
            action.AddParameter("entity", new EdmEntityTypeReference(vipCustomer, false));
            action.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(action);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataTypeCastSegment(vipCustomer),
                new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.NS.VipCustomer.MyAction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OperationRestrictionsTermWorksToCreateOperationForEdmAction(bool enableAnnotation)
        {
            string template = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""user"" />
      <Action Name=""getMemberGroups"" IsBound=""true"">
        <Parameter Name=""bindingParameter"" Type=""NS.user"" Nullable=""false"" />
          {0}
      </Action>
      <EntityContainer Name=""GraphService"">
        <Singleton Name=""me"" Type=""NS.user"" />
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
  </Record>
</Annotation>";

            // Arrange
            string csdl = string.Format(template, enableAnnotation ? annotation : "");

            var edmModel = CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            Assert.NotNull(edmModel);
            IEdmSingleton me = edmModel.EntityContainer.FindSingleton("me");
            Assert.NotNull(me);
            IEdmAction action = edmModel.SchemaElements.OfType<IEdmAction>().SingleOrDefault();
            Assert.NotNull(action);
            Assert.Equal("getMemberGroups", action.Name);

            ODataContext context = new ODataContext(edmModel);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataOperationSegment(action));

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
            }
            else
            {
                Assert.Empty(operation.Security);
            }
        }
    }
}
