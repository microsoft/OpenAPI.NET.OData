// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class SingletonPatchOperationHandlerTests
    {
        private SingletonPatchOperationHandler _operationHandler = new SingletonPatchOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateSingletonPatchOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            string annotation = @"
        <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
          <Record>
            <PropertyValue Property=""Description"" String=""Update the signed-in user."" />            
            <PropertyValue Property=""LongDescription"" String=""Update the signed-in user."" />            
          </Record>
        </Annotation>";
            IEdmModel model = SingletonGetOperationHandlerTests.GetEdmModel(annotation);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var patch = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(patch);
            Assert.Equal("Update the signed-in user.", patch.Summary);
            Assert.Equal("Update the signed-in user.", patch.Description);
            Assert.NotNull(patch.Tags);
            var tag = Assert.Single(patch.Tags);
            Assert.Equal("Me.Customer", tag.Name);

            Assert.Empty(patch.Parameters);
            Assert.NotNull(patch.RequestBody);

            Assert.NotNull(patch.Responses);
            Assert.Equal(2, patch.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "204";
            Assert.Equal(new[] { statusCode, "default" }, patch.Responses.Select(r => r.Key));

            if (useHTTPStatusCodeClass2XX)
            {
                Assert.Single(patch.Responses[statusCode].Content);
            }
            else
            {
                Assert.Empty(patch.Responses[statusCode].Content);
            }

            if (enableOperationId)
            {
                Assert.Equal("Me.Customer.UpdateCustomer", patch.OperationId);
            }
            else
            {
                Assert.Null(patch.OperationId);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateSingletonPatchOperationReturnsParameterForUpdateRestrictions(bool hasRestriction)
        {
            // Arrange
            string annotation = @"<Annotations Target=""NS.Default/Me"">
                <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Updatable"" Bool=""true"" />
                    <PropertyValue Property=""NonUpdatableNavigationProperties"" >
                      <Collection>
                        <NavigationPropertyPath>abc</NavigationPropertyPath>
                        <NavigationPropertyPath>RelatedEvents</NavigationPropertyPath>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""MaxLevels"" Int=""8"" />
                    <PropertyValue Property=""Permissions"">
                      <Collection>
                        <Record Type=""Org.OData.Capabilities.V1.PermissionType"">
                          <PropertyValue Property=""SchemeName"" String=""authorizationName"" />
                          <PropertyValue Property=""Scopes"">
                            <Collection>
                              <Record Type=""Org.OData.Capabilities.V1.ScopeType"">
                                <PropertyValue Property=""Scope"" String=""scopeName1"" />
                                <PropertyValue Property=""RestrictedProperties"" String=""p1,p2"" />
                              </Record>
                              <Record Type=""Org.OData.Capabilities.V1.ScopeType"">
                                <PropertyValue Property=""Scope"" String=""scopeName2"" />
                                <PropertyValue Property=""RestrictedProperties"" String=""p3,p4"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""QueryOptions"">
                      <Record>
                        <PropertyValue Property=""ExpandSupported"" Bool=""true"" />
                        <PropertyValue Property=""SelectSupported"" Bool=""true"" />
                        <PropertyValue Property=""ComputeSupported"" Bool=""true"" />
                        <PropertyValue Property=""FilterSupported"" Bool=""true"" />
                        <PropertyValue Property=""SearchSupported"" Bool=""true"" />
                        <PropertyValue Property=""SortSupported"" Bool=""false"" />
                        <PropertyValue Property=""SortSupported"" Bool=""false"" />
                      </Record>
                    </PropertyValue>
                    <PropertyValue Property=""CustomHeaders"">
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""HeadName1"" />
                          <PropertyValue Property=""Description"" String=""Description1"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any1"" />
                          <PropertyValue Property=""Required"" Bool=""true"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description11"" />
                                <PropertyValue Property=""Value"" String=""value1"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                        <Record>
                          <PropertyValue Property=""Name"" String=""HeadName2"" />
                          <PropertyValue Property=""Description"" String=""Description2"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any2"" />
                          <PropertyValue Property=""Required"" Bool=""false"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description22"" />
                                <PropertyValue Property=""Value"" String=""value2"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                      </Collection>
                    </PropertyValue>
                    <PropertyValue Property=""CustomQueryOptions"">
                      <Collection>
                        <Record>
                          <PropertyValue Property=""Name"" String=""QueryName1"" />
                          <PropertyValue Property=""Description"" String=""Description3"" />
                          <PropertyValue Property=""ComputeSupported"" String=""http://any3"" />
                          <PropertyValue Property=""Required"" Bool=""true"" />
                          <PropertyValue Property=""ExampleValues"">
                            <Collection>
                              <Record>
                                <PropertyValue Property=""Description"" String=""Description33"" />
                                <PropertyValue Property=""Value"" String=""value3"" />
                              </Record>
                            </Collection>
                          </PropertyValue>
                        </Record>
                      </Collection>
                    </PropertyValue>
                  </Record>
                </Annotation>
              </Annotations>";

            // Act & Assert
            VerifyOperation(annotation, hasRestriction);
        }

        private void VerifyOperation(string annotation, bool hasRestriction)
        {
            // Arrange
            IEdmModel model = CapabilitiesModelHelper.GetEdmModelOutline(hasRestriction ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmSingleton me = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(me); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(me));

            // Act
            var patch = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(patch);

            Assert.NotNull(patch.Parameters);
            if (hasRestriction)
            {
                // Parameters
                Assert.Equal(3, patch.Parameters.Count);

                Assert.Equal(ParameterLocation.Header, patch.Parameters[0].In);
                Assert.Equal("HeadName1", patch.Parameters[0].Name);

                Assert.Equal(ParameterLocation.Header, patch.Parameters[1].In);
                Assert.Equal("HeadName2", patch.Parameters[1].Name);

                Assert.Equal(ParameterLocation.Query, patch.Parameters[2].In);
                Assert.Equal("QueryName1", patch.Parameters[2].Name);

                // security
                Assert.NotNull(patch.Security);
                var securityRequirements = Assert.Single(patch.Security);
                var securityRequirement = Assert.Single(securityRequirements);
                Assert.Equal("authorizationName", securityRequirement.Key.Reference.Id);
                Assert.Equal(new[] { "scopeName1", "scopeName2" }, securityRequirement.Value);

                string json = patch.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                Assert.Contains(@"
  ""security"": [
    {
      ""authorizationName"": [
        ""scopeName1"",
        ""scopeName2""
      ]
    }
  ],".ChangeLineBreaks(), json);

                // with custom header
                Assert.Contains(@"
  ""parameters"": [
    {
      ""name"": ""HeadName1"",
      ""in"": ""header"",
      ""description"": ""Description1"",
      ""required"": true,
      ""schema"": {
        ""type"": ""string""
      },
      ""examples"": {
        ""example-1"": {
          ""description"": ""Description11"",
          ""value"": ""value1""
        }
      }
    },
    {
      ""name"": ""HeadName2"",
      ""in"": ""header"",
      ""description"": ""Description2"",
      ""schema"": {
        ""type"": ""string""
      },
      ""examples"": {
        ""example-1"": {
          ""description"": ""Description22"",
          ""value"": ""value2""
        }
      }
    },
    {
      ""name"": ""QueryName1"",
      ""in"": ""query"",
      ""description"": ""Description3"",
      ""required"": true,
      ""schema"": {
        ""type"": ""string""
      },
      ""examples"": {
        ""example-1"": {
          ""description"": ""Description33"",
          ""value"": ""value3""
        }
      }
    }".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Empty(patch.Parameters);
                Assert.Empty(patch.Security);
            }
        }
    }
}
