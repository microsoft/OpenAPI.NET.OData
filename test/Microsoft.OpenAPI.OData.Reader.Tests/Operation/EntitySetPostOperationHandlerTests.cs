// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntitySetPostOperationHandlerTests
    {
        private EntitySetPostOperationHandler _operationHandler = new EntitySetPostOperationHandler();

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void CreateEntitySetPostOperationReturnsCorrectOperation(bool enableOperationId, bool hasStream, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            string qualifiedName = CoreConstants.AcceptableMediaTypes;
            string annotation = $@"
            <Annotation Term=""{qualifiedName}"" >
              <Collection>
                <String>application/todo</String>
              </Collection>
            </Annotation>";

            // Assert
            VerifyEntitySetPostOperation("", enableOperationId, hasStream, useHTTPStatusCodeClass2XX);
            VerifyEntitySetPostOperation(annotation, enableOperationId, hasStream, useHTTPStatusCodeClass2XX);
        }

        private void VerifyEntitySetPostOperation(string annotation, bool enableOperationId, bool hasStream, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = GetEdmModel(annotation, hasStream);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var post = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(post);
            Assert.Equal("Create customer.", post.Summary);
            Assert.Equal("Create a new customer.", post.Description);
            Assert.NotNull(post.Tags);
            var tag = Assert.Single(post.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.Empty(post.Parameters);
            Assert.NotNull(post.RequestBody);

            Assert.NotNull(post.Responses);
            Assert.Equal(2, post.Responses.Count);

            var statusCode = useHTTPStatusCodeClass2XX ? Constants.StatusCodeClass2XX : Constants.StatusCode201;
            if (hasStream)
            {
                Assert.NotNull(post.RequestBody);

                if (!string.IsNullOrEmpty(annotation))
                {
                    // RequestBody
                    Assert.Single(post.RequestBody.Content.Keys);
                    Assert.True(post.RequestBody.Content.ContainsKey("application/todo"));

                    // Response
                    Assert.Single(post.Responses[statusCode].Content.Keys);
                    Assert.True(post.Responses[statusCode].Content.ContainsKey("application/todo"));
                }
                else
                {
                    // RequestBody
                    Assert.Single(post.RequestBody.Content.Keys);
                    Assert.True(post.RequestBody.Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));

                    // Response
                    Assert.Single(post.Responses[statusCode].Content.Keys);
                    Assert.True(post.Responses[statusCode].Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
                }
            }
            else
            {
                // RequestBody
                Assert.NotNull(post.RequestBody);
                Assert.Single(post.RequestBody.Content.Keys);
                Assert.True(post.RequestBody.Content.ContainsKey(Constants.ApplicationJsonMediaType));

                // Response
                Assert.Single(post.Responses[Constants.StatusCode201].Content.Keys);
                Assert.True(post.Responses[Constants.StatusCode201].Content.ContainsKey(Constants.ApplicationJsonMediaType));
            }

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.CreateCustomer", post.OperationId);
            }
            else
            {
                Assert.Null(post.OperationId);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntitySetPostReturnsSecurityForInsertRestrictions(bool enableAnnotation)
        {
            string annotation = @"<Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
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
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers));

            // Act
            var post = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(post);
            Assert.NotNull(post.Security);

            if (enableAnnotation)
            {
                Assert.Equal(2, post.Security.Count);

                string json = post.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
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

                // Parameters
                Assert.Single(post.Parameters);

                Assert.Equal(ParameterLocation.Header, post.Parameters[0].In);
                Assert.Equal("odata-debug", post.Parameters[0].Name);
            }
            else
            {
                Assert.Empty(post.Security);
            }
        }

        [Fact]
        public void CreateEntitySetPostOperationReturnsCorrectOperationWithAnnotatedRequestBodyAndResponseContent()
        {
            IEdmModel model = OData.Tests.EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new();
            ODataContext context = new(model, settings);
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("directoryObjects");
            Assert.NotNull(entitySet);

            ODataPath path = new(new ODataNavigationSourceSegment(entitySet));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation.RequestBody);
            Assert.Equal("multipart/form-data", operation.RequestBody.Content.First().Key);
            Assert.NotNull(operation.Responses);
            Assert.Equal("multipart/form-data", operation.Responses.First().Value.Content.First().Key);
        }

        internal static IEdmModel GetEdmModel(string annotation, bool hasStream = false)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <ComplexType Name=""Address"">
        <Property Name=""City"" Type=""Edm.String"" />
      </ComplexType>
      <EntityType Name=""Customer"" HasStream=""{0}"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""BillingAddress"" Type=""NS.Address"" />
        <Property Name=""MailingAddress"" Type=""NS.Address"" Nullable=""false"" />
        <Property Name=""AlternativeAddresses"" Type=""Collection(NS.Address)"" Nullable=""false"">
            <Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
                <Record>
                    <PropertyValue Property=""Description"" String=""Create AlternativeAddress."" />            
                    <PropertyValue Property=""LongDescription"" String=""Create a new AlternativeAddress."" />            
                </Record>
            </Annotation>
        </Property>
      </EntityType>
      <EntityContainer Name =""Default"">
        <EntitySet Name=""Customers"" EntityType=""NS.Customer"">
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""Collection of business customers."" />
            <Annotation Term=""Org.OData.Core.V1.LongDescription"" String=""Collection of business customers."" />
        </EntitySet>
      </EntityContainer>
      <Annotations Target=""NS.Customer"">
       {1}
      </Annotations>
      <Annotations Target=""NS.Default/Customers"">
        <Annotation Term=""Org.OData.Capabilities.V1.InsertRestrictions"">
          <Record>
            <PropertyValue Property=""Description"" String=""Create customer."" />            
            <PropertyValue Property=""LongDescription"" String=""Create a new customer."" />            
          </Record>
        </Annotation>
      </Annotations>        
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            string modelText = string.Format(template, hasStream, annotation);
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }
    }
}
