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
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityPutOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var putOperation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(putOperation);
            Assert.Equal("Update entity in Customers", putOperation.Summary);
            Assert.Equal("A business customer.", putOperation.Description);
            Assert.NotNull(putOperation.Tags);
            var tag = Assert.Single(putOperation.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(putOperation.Parameters);
            Assert.Equal(1, putOperation.Parameters.Count);

            Assert.NotNull(putOperation.RequestBody);

            Assert.NotNull(putOperation.Responses);
            Assert.Equal(2, putOperation.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, putOperation.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.UpdateCustomer", putOperation.OperationId);
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
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers), new ODataKeySegment(customers.EntityType()));

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
    }
}
