// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class SingletonGetOperationHandlerTests
    {
        private SingletonGetOperationHandler _operationHandler = new SingletonGetOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateSingletonGetOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            string annotation = @"
        <Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
          <Record>
            <PropertyValue Property=""Description"" String=""List users."" />            
            <PropertyValue Property=""LongDescription"" String=""Retrieve a list of user objects."" />            
          </Record>
        </Annotation>";
            IEdmModel model = GetEdmModel(annotation);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Me");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(singleton));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("List users.", get.Summary);
            Assert.Equal("Retrieve a list of user objects.", get.Description);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Me.Customer", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(2, get.Parameters.Count);

            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new[] { statusCode, "default" }, get.Responses.Select(r => r.Key));

            if (enableOperationId)
            {
                Assert.Equal("Me.Customer.GetCustomer", get.OperationId);
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
        public void CreateSingletonGetOperationReturnsParameterForExpandRestrictions(bool hasRestriction, bool expandable)
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
        public void CreateSingletonGetOperationReturnsParameterForNavigationRestrictions(bool hasRestriction, string navigability)
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
        public void ReadRestrictionsTermWorksToCreateOperationForSingletonGetOperation(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
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
</Annotation>";

            // Arrange
            var edmModel = GetEdmModel(enableAnnotation ? annotation : "");

            Assert.NotNull(edmModel);
            IEdmSingleton me = edmModel.EntityContainer.FindSingleton("Me");
            Assert.NotNull(me);

            ODataContext context = new ODataContext(edmModel);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(me));

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
  ],".ChangeLineBreaks(), json);

                // with custom header
                Assert.Contains(@"
  ""parameters"": [
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
    },
    {".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Empty(operation.Security);
            }
        }

        public static IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
      </EntityType>
      <EntityContainer Name =""Default"">
        <Singleton Name=""Me"" Type=""NS.Customer"">
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""My signed-in instance."" />
        </Singleton>
      </EntityContainer>
      <Annotations Target=""NS.Default/Me"">
        {0}
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            IEdmModel model;
            IEnumerable<EdmError> errors;

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out errors);
            Assert.True(result);
            return model;
        }

        private void VerifyParameter(string annotation, bool hasRestriction, bool supported, string queryOption)
        {
            // Arrange
            IEdmModel model = GetEdmModel(hasRestriction ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmSingleton me = model.EntityContainer.FindSingleton("Me");
            Assert.NotNull(me); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(me));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);

            Assert.NotNull(get.Parameters);
            if (!hasRestriction || supported)
            {
                Assert.Equal(2, get.Parameters.Count);
                Assert.Contains(queryOption, get.Parameters.Select(p => p.Name));
            }
            else
            {
                Assert.Single(get.Parameters);
                Assert.DoesNotContain(queryOption, get.Parameters.Select(p => p.Name));
            }
        }
    }
}
