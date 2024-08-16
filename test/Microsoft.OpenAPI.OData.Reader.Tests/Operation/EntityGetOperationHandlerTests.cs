// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntityGetOperationHandlerTests
    {
        private EntityGetOperationHandler _operationHandler = new EntityGetOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateEntityGetOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
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
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get customer.", get.Summary);
            Assert.Equal("Returns a single customer.", get.Description);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(3, get.Parameters.Count);
            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new[] { statusCode, "default" }, get.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.GetCustomer", get.OperationId);
            }
            else
            {
                Assert.Null(get.OperationId);
            }
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntityGetOperationReturnsParameterForExpandRestrictions(bool hasRestriction, bool expandable)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.ExpandRestrictions"">
  <Record>
    <PropertyValue Property=""Expandable"" Bool=""{0}"" />
  </Record>
</Annotation>", expandable);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, expandable, "$expand");
        }

        [Theory]
        [InlineData(false, "Recursive")]
        [InlineData(false, "Single")]
        [InlineData(false, "None")]
        [InlineData(true, "Recursive")]
        [InlineData(true, "Single")]
        [InlineData(true, "None")]
        public void CreateEntityGetOperationReturnsParameterForNavigationRestrictions(bool hasRestriction, string navigability)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
  <Record>
    <PropertyValue Property=""Navigability"">
      <EnumMember>Org.OData.Capabilities.V1.NavigationType/{0}</EnumMember >
    </PropertyValue>
  </Record>
</Annotation>", navigability);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, navigability == "None" ? false : true, "$select");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityGetOperationReturnsSecurityForReadRestrictions(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
    <PropertyValue Property=""ReadByKeyRestrictions"">
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
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.NotNull(get.Security);

            if (enableAnnotation)
            {
                Assert.Equal(2, get.Security.Count);

                string json = get.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
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

                Assert.Contains(get.Parameters, p => p.Name == "odata-debug" && p.In == Models.ParameterLocation.Header);
            }
            else
            {
                Assert.Empty(get.Security);
            }
        }

        private void VerifyParameter(string annotation, bool hasRestriction, bool supported, string queryOption)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel(hasRestriction ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers), new ODataKeySegment(customers.EntityType));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);

            Assert.NotNull(get.Parameters);
            if (!hasRestriction || supported)
            {
                Assert.Equal(3, get.Parameters.Count);
                Assert.Contains(queryOption, get.Parameters.Select(p => p.Name));
            }
            else
            {
                Assert.Equal(2, get.Parameters.Count);
                Assert.DoesNotContain(queryOption, get.Parameters.Select(p => p.Name));
            }
        }
    }
}
