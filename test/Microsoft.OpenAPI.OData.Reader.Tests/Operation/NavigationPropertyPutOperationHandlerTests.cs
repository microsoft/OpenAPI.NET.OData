﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.PathItem.Tests;
using Microsoft.OpenApi.OData.Tests;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class NavigationPropertyPutOperationHandlerTests
    {
        public NavigationPropertyPutOperationHandlerTests()
        {
          _openApiDocument.AddComponent("Delegated (work or school account)", new OpenApiSecurityScheme {
            Type = SecuritySchemeType.OAuth2,
          });
          _openApiDocument.AddComponent("Application", new OpenApiSecurityScheme {
            Type = SecuritySchemeType.OAuth2,
          });
        }
        private readonly OpenApiDocument _openApiDocument = new();
        private NavigationPropertyPutOperationHandler _operationHandler => new(_openApiDocument);

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateNavigationPutOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "BestFriend");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Update the best friend.", operation.Summary);
            Assert.Equal("Update an instance of a best friend.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Single(operation.Parameters);

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("New navigation property values", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));

            if (useHTTPStatusCodeClass2XX)
            {
                Assert.Single(operation.Responses[statusCode].Content);
            }
            else
            {
                Assert.Null(operation.Responses[statusCode].Content);
            }

            if (enableOperationId)
            {
                Assert.Equal("People.SetBestFriend", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateNavigationPuthOperationReturnsSecurityForUpdateRestrictions(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
   <PropertyValue Property=""RestrictedProperties"" >
      <Collection>
        <Record>
          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""Orders"" />
          <PropertyValue Property=""UpdateRestrictions"" >
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
              <PropertyValue Property=""Description"" String=""A brief description of GET '/me' request."" />
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
          </PropertyValue>
        </Record>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>";

            // Arrange
            IEdmModel edmModel = NavigationPropertyPathItemHandlerTest.GetEdmModel(enableAnnotation ? annotation : "");
            Assert.NotNull(edmModel);
            ODataContext context = new ODataContext(edmModel);
            IEdmEntitySet entitySet = edmModel.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(entitySet); // guard
            IEdmEntityType entityType = entitySet.EntityType;

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties().FirstOrDefault(c => c.Name == "Orders");
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property),
                new ODataKeySegment(property.DeclaringEntityType()));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation);

            if (enableAnnotation)
            {
              Assert.NotNull(operation.Security);
                Assert.Equal(2, operation.Security.Count);

                string json = await operation.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
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

                // with custom header
                Assert.Contains(@"
    {
      ""name"": ""odata-debug"",
      ""in"": ""header"",
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
    }".ChangeLineBreaks(), json);

            }
            else
            {
                Assert.Null(operation.Security);
            }
        }
    }
}
