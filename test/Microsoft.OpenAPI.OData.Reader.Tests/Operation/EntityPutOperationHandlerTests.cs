// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntityPutOperationHandlerTests
    {
        private EntityPutOperationHandler _operationHandler = new EntityPutOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateEntityPutOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var putOperation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(putOperation);
            Assert.Equal("Update customer.", putOperation.Summary);
            Assert.Equal("Updates a single customer.", putOperation.Description);
            Assert.NotNull(putOperation.Tags);
            var tag = Assert.Single(putOperation.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(putOperation.Parameters);
            Assert.Single(putOperation.Parameters);

            Assert.NotNull(putOperation.RequestBody);

            Assert.NotNull(putOperation.Responses);
            Assert.Equal(2, putOperation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
            Assert.Equal(new[] { statusCode, "default" }, putOperation.Responses.Select(r => r.Key));

            if (useHTTPStatusCodeClass2XX)
            {
                Assert.Single(putOperation.Responses[statusCode].Content);
            }
            else
            {
                Assert.Empty(putOperation.Responses[statusCode].Content);
            }

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.SetCustomer", putOperation.OperationId);
            }
            else
            {
                Assert.Null(putOperation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityPutReturnsSecurityForUpdateRestrictions(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
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
    <PropertyValue Property=""CustomHeaders"">
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
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel(enableAnnotation ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers), new ODataKeySegment(customers.EntityType));

            // Act
            var putOperation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(putOperation);
            Assert.NotNull(putOperation.Security);

            if (enableAnnotation)
            {
                Assert.Equal(2, putOperation.Security.Count);

                string json = putOperation.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                Assert.Contains(@"
  ""security"": [
    {
      ""Delegated (work or school account)"": [
        ""User.ReadBasic.All"",
        ""User.Read.All""
      ]
    },
    {
      ""Application"": [
        ""User.Read.All"",
        ""Directory.Read.All""
      ]
    }
  ],".ChangeLineBreaks(), json);

                Assert.Contains(putOperation.Parameters, p => p.Name == "odata-debug" && p.In == Models.ParameterLocation.Header);
            }
            else
            {
                Assert.Empty(putOperation.Security);
            }
        }

        [Fact]
        public void CreateEntityPutOperationReturnsCorrectOperationWithAnnotatedRequestBodyContent()
        {
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new();
            ODataContext context = new(model, settings);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("directoryObjects");
            Assert.NotNull(entitySet);

            ODataPath path = new(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation.RequestBody);
            Assert.Equal("multipart/form-data", operation.RequestBody.Content.First().Key);
        }
    }
}
