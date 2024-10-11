// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EdmFunctionOperationHandlerTests
    {
        private EdmFunctionOperationHandler _operationHandler = new();
        #region OperationHandlerTests
        [Fact]
        public void SetsDeprecationInformation()
        {
          // Arrange
          IEdmModel model = EdmModelHelper.TripServiceModel;
          ODataContext context = new(model);
          IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
          Assert.NotNull(people);

          IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "GetFriendsTrips");
          Assert.NotNull(function);

          ODataPath path = new(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataOperationSegment(function));

          // Act
          var operation = _operationHandler.CreateOperation(context, path);

          // Assert
          Assert.NotNull(operation);
          Assert.True(operation.Deprecated);
          Assert.NotEmpty(operation.Extensions);
          var extension = operation.Extensions["x-ms-deprecation"];
          Assert.NotNull(extension);
        }

        [Fact]
        public void DoesntSetDeprecationInformation()
        {
          // Arrange
          IEdmModel model = EdmModelHelper.TripServiceModel;
          ODataContext context = new(model);
          IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
          Assert.NotNull(people);

          IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "GetFavoriteAirline");
          Assert.NotNull(function);

          ODataPath path = new(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataOperationSegment(function));

          // Act
          var operation = _operationHandler.CreateOperation(context, path);

          // Assert
          Assert.NotNull(operation);
          Assert.True(operation.Deprecated);
          Assert.NotEmpty(operation.Extensions);
          var extension = operation.Extensions["x-ms-deprecation"];
          Assert.NotNull(extension);
        }

        #endregion

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionReturnsCorrectOperation(bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            var settings = new OpenApiConvertSettings
            {
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "GetFavoriteAirline");
            Assert.NotNull(function);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Invoke function GetFavoriteAirline", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Single(operation.Parameters);
            Assert.Equal(new string[] { "UserName" }, operation.Parameters.Select(p => p.Name));

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));
        }

        [Fact]
        public void CreateOperationForEdmFunctionReturnsCorrectOperationHierarchicalClass()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.ContractServiceModel;
            ODataContext context = new ODataContext(model);
            const string entitySetName = "Accounts";
            const string functionName = "Attachments";
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(entitySetName);
            Assert.NotNull(entitySet);

            IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == functionName);
            Assert.NotNull(function);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType), new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal($"Invoke function {functionName}", operation.Summary);
            Assert.Equal("Collection of contract attachments.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal($"{entitySetName}.AccountApiModel", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(6, operation.Parameters.Count); // id, top, skip, count, search, filter
            Assert.Contains("id", operation.Parameters.Select(x => x.Name).FirstOrDefault());

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "200", "default" }, operation.Responses.Select(e => e.Key));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
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
                new ODataOperationSegment(function));

            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(customers),
                new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            var operation2 = _operationHandler.CreateOperation(context, path2);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.MyFunction", operation.OperationId);
                Assert.Equal("Customers.MyFunction", operation2.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
                Assert.Null(operation2.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForEdmFunctionWithTypeCastReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(vipCustomer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
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
                new ODataTypeCastSegment(vipCustomer, model),
                new ODataOperationSegment(function));
          
            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.NS.VipCustomer.MyFunction", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForOverloadEdmFunctionReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);

            function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            function.AddParameter("param2", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                AddSingleQuotesForStringParameters = true,
            };
            ODataContext context = new ODataContext(model, settings);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.MyFunction-df74", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOperationForComposableOverloadEdmFunctionReturnsCorrectOperationId(bool enableOperationId)
        {
            // Arrange
            EdmModel model = new();
            EdmEntityType customer = new("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);

            // Overloaded function 1 
            EdmFunction function1 = new("NS", "MyFunction1", EdmCoreModel.Instance.GetString(false), true, null, false);
            function1.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(function1);

            // Overloaded function 1
            EdmFunction function2 = new("NS", "MyFunction1", EdmCoreModel.Instance.GetString(false), true, null, false);
            function2.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function2.AddParameter("param", EdmCoreModel.Instance.GetString(false));

            model.AddElement(function2);

            // Overloaded function 2
            EdmFunction function3 = new("NS", "MyFunction2", EdmCoreModel.Instance.GetString(false), true, null, false);
            function3.AddParameter("entity2", new EdmEntityTypeReference(customer, false));
            model.AddElement(function3);

            // Overloaded function 2
            EdmFunction function4 = new("NS", "MyFunction2", EdmCoreModel.Instance.GetString(false), true, null, false);
            function4.AddParameter("entity2", new EdmEntityTypeReference(customer, false));
            function4.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function4);

            EdmEntityContainer container = new("NS", "Default");
            EdmEntitySet customers = new(container, "Customers", customer);
            model.AddElement(container);

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                AddSingleQuotesForStringParameters = true,
            };
            ODataContext context = new(model, settings);

            ODataPath path1 = new(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(function1),
                new ODataOperationSegment(function3));

            ODataPath path2 = new(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(function1),
                new ODataOperationSegment(function4));

            ODataPath path3 = new(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(function2),
                new ODataOperationSegment(function3));

            ODataPath path4 = new(new ODataNavigationSourceSegment(customers),
                new ODataKeySegment(customer),
                new ODataOperationSegment(function2),
                new ODataOperationSegment(function4));

            // Act
            var operation1 = _operationHandler.CreateOperation(context, path1);
            var operation2 = _operationHandler.CreateOperation(context, path2);
            var operation3 = _operationHandler.CreateOperation(context, path3);
            var operation4 = _operationHandler.CreateOperation(context, path4);

            // Assert
            Assert.NotNull(operation1);
            Assert.NotNull(operation2);
            Assert.NotNull(operation3);
            Assert.NotNull(operation4);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.MyFunction1.MyFunction2-c53d", operation1.OperationId);
                Assert.Equal("Customers.Customer.MyFunction1.MyFunction2-4d93", operation2.OperationId);
                Assert.Equal("Customers.Customer.MyFunction1.MyFunction2-a2b2", operation3.OperationId);
                Assert.Equal("Customers.Customer.MyFunction1.MyFunction2-7bea", operation4.OperationId);
            }
            else
            {
                Assert.Null(operation1.OperationId);
                Assert.Null(operation2.OperationId);
                Assert.Null(operation3.OperationId);
                Assert.Null(operation4.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OperationRestrictionsTermWorksToCreateOperationForEdmFunction(bool enableAnnotation)
        {
            string template = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""user"" />
      <Function Name=""getMemberGroups"" IsBound=""true"">
        <Parameter Name=""bindingParameter"" Type=""NS.user"" Nullable=""false"" />
        <ReturnType Type=""Edm.String"" Nullable=""false"" />
          {0}
      </Function>
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
            IEdmFunction function = edmModel.SchemaElements.OfType<IEdmFunction>().SingleOrDefault();
            Assert.NotNull(function);
            Assert.Equal("getMemberGroups", function.Name);

            ODataContext context = new ODataContext(edmModel);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataOperationSegment(function));

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

        [Theory]
        [InlineData("getUserArchivedPrintJobs", true)] // returns collection
        [InlineData("getUserArchivedPrintJobs", false)] // returns collection
        [InlineData("managedDeviceEnrollmentAbandonmentSummary", true)] // does not return collection
        public void CreateOperationForEdmFunctionWithCollectionReturnTypeContainsXMsPageableExtension(string functionName, bool enablePagination)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = enablePagination
            };
            ODataContext context = new(model, settings);
            IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>()
                .First(x => x.Name == functionName &&
                    x.FindParameter("bindingParameter").Type.Definition.ToString() == "microsoft.graph.reportRoot");
            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().First();
            IEdmSingleton reports = container.FindSingleton("reports");

            ODataPath path = new(new ODataNavigationSourceSegment(reports),
                new ODataOperationSegment(function));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            if (enablePagination && function.ReturnType.IsCollection())
            {
                Assert.True(operation.Extensions.ContainsKey(Common.Constants.xMsPageable));
            }
            else
            {
                Assert.False(operation.Extensions.ContainsKey(Common.Constants.xMsPageable));
            }
        }

        [Fact]
        public void CreateOperationForFunctionWithDateTimeParametersReturnsCorrectPathItemName()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                AddSingleQuotesForStringParameters = true,
            };
            ODataContext context = new(model, settings);

            IEdmFunction function = model.SchemaElements.OfType<IEdmFunction>()
                .FirstOrDefault(x => x.Name == "getPrinterUsageSummary");
            Assert.NotNull(function); // guard

            IEdmEntityContainer container = model.SchemaElements.OfType<IEdmEntityContainer>().First();
            IEdmSingleton reports = container.FindSingleton("reports");

            ODataPath path = new(new ODataNavigationSourceSegment(reports),
                new ODataOperationSegment(function));

            // Act
            OpenApiOperation operation = _operationHandler.CreateOperation(context, path);
            string pathItemName = path.GetPathItemName();
            
            // Assert
            Assert.NotNull(operation);
            Assert.Equal("/reports/microsoft.graph.getPrinterUsageSummary(printerId={printerId},periodStart={periodStart},periodEnd={periodEnd})", pathItemName);
            Assert.Equal("Usage: periodStart={periodStart}", operation.Parameters.First(x => x.Name == "periodStart").Description);
            Assert.Equal("Usage: periodEnd={periodEnd}", operation.Parameters.First(x => x.Name == "periodEnd").Description);

        }

        [Fact]
        public void CreateFunctionOperationWithAlternateKeyReturnsCorrectOperationId()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings()
            {
                EnableOperationId = true
            });

            IEdmSingleton singleton = model.EntityContainer.FindSingleton("communications");
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "cloudCommunications");
            IEdmNavigationProperty navProp = entityType.DeclaredNavigationProperties().First(c => c.Name == "onlineMeetings");
            IEdmOperation action = model.SchemaElements.OfType<IEdmOperation>().First(f => f.Name == "sendVirtualAppointmentReminderSms");
            IDictionary<string, string> keyMappings = new Dictionary<string, string> { { "joinWebUrl", "joinWebUrl" } };

            ODataPath path = new(new ODataNavigationSourceSegment(singleton),
                new ODataNavigationPropertySegment(navProp),
                new ODataKeySegment(entityType, keyMappings)
                {
                    IsAlternateKey = true
                },
                new ODataOperationSegment(action));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("communications.onlineMeetings.joinWebUrl.sendVirtualAppointmentReminderSms", operation.OperationId);
        }
    }
}
