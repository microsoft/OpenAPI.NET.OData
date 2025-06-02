﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntityDeleteOperationHandlerTests
    {
        public EntityDeleteOperationHandlerTests()
        {
          _openApiDocument.AddComponent("Delegated (work or school account)", new OpenApiSecurityScheme {
            Type = SecuritySchemeType.OAuth2,
          });
          _openApiDocument.AddComponent("Application", new OpenApiSecurityScheme {
            Type = SecuritySchemeType.OAuth2,
          });
          _openApiDocument.Tags ??= new HashSet<OpenApiTag>();
          _openApiDocument.Tags.Add(new OpenApiTag { Name = "Customers.Customer" });
        }
        private readonly OpenApiDocument _openApiDocument = new();
        private EntityDeleteOperationHandler _operationHandler => new (_openApiDocument);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityDeleteOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType));

            // Act
            var delete = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(delete);
            Assert.Equal("Delete customer.", delete.Summary);
            Assert.Equal("Deletes a single customer.", delete.Description);
            Assert.NotNull(delete.Tags);
            var tag = Assert.Single(delete.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(delete.Parameters);
            Assert.Equal(2, delete.Parameters.Count);

            Assert.Null(delete.RequestBody);

            Assert.NotNull(delete.Responses);
            Assert.Equal(2, delete.Responses.Count);
            Assert.Equal(new[] { "204", "default" }, delete.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.DeleteCustomer", delete.OperationId);
            }
            else
            {
                Assert.Null(delete.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateEntityDeleteReturnsSecurityForDeleteRestrictions(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
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
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel(enableAnnotation ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers), new ODataKeySegment(customers.EntityType));

            // Act
            var delete = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(delete);

            if (enableAnnotation)
            {
              Assert.NotNull(delete.Security);
                Assert.Equal(2, delete.Security.Count);

                string json = await delete.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
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

                Assert.Contains(delete.Parameters, p => p.Name == "odata-debug" && p.In == ParameterLocation.Query);
            }
            else
            {
                Assert.Null(delete.Security);
            }
        }
    }
}
