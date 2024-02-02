﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.PathItem.Tests;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class NavigationPropertyGetOperationHandlerTests
    {
        private NavigationPropertyGetOperationHandler _operationHandler = new NavigationPropertyGetOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateNavigationGetOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
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
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Trips");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType()), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("List trips.", operation.Summary);
            Assert.Equal("Retrieve a list of trips.", operation.Description);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Trip", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(10, operation.Parameters.Count);

            Assert.Null(operation.RequestBody);

            Assert.Equal(2, operation.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new string[] { statusCode, "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.ListTrips", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }

        [Fact]
        public void CreateNavigationGetOperationViaComposableFunctionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings()
            {
                EnableOperationId = true
            });

            IEdmEntitySet sites = model.EntityContainer.FindEntitySet("sites");
            IEdmEntityType site = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "site");
            IEdmNavigationProperty analytics = site.DeclaredNavigationProperties().First(c => c.Name == "analytics");
            IEdmOperation getByPath = model.SchemaElements.OfType<IEdmOperation>().First(f => f.Name == "getByPath");

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(sites),
                new ODataKeySegment(site),
                new ODataOperationSegment(getByPath),
                new ODataNavigationPropertySegment(analytics));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("sites.getByPath.GetAnalytics", operation.OperationId);
            Assert.NotNull(operation.Parameters);
            Assert.Equal(4, operation.Parameters.Count);
            Assert.Contains(operation.Parameters, x => x.Name == "path");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateNavigationGetOperationReturnsSecurityForReadRestrictions(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
   <PropertyValue Property=""RestrictedProperties"" >
      <Collection>
        <Record>
          <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""Orders"" />
          <PropertyValue Property=""ReadRestrictions"" >
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
            IEdmEntityType entityType = entitySet.EntityType();

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties().FirstOrDefault(c => c.Name == "Orders");
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

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
        ""User.Read.All""
      ]
    },
    {
      ""Application"": [
        ""User.Read.All"",
        ""Directory.Read.All""
      ]
    }
  ]".ChangeLineBreaks(), json);

                // with custom header
                Assert.Contains(@"
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
    }".ChangeLineBreaks(), json);

            }
            else
            {
                Assert.Empty(operation.Security);
            }
        }
    }
}
