// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntitySetGetOperationHandlerTests
    {
        private EntitySetGetOperationHandler _operationHandler = new EntitySetGetOperationHandler();

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        public void CreateEntitySetGetOperationReturnsCorrectOperation(bool enableOperationId, bool enablePagination, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                EnablePagination = enablePagination,
                UseSuccessStatusCodeRange = useHTTPStatusCodeClass2XX
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("List customers.", get.Summary);
            Assert.Equal("Returns a list of customers.", get.Description);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(8, get.Parameters.Count);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);

            var statusCode = useHTTPStatusCodeClass2XX ? "2XX" : "200";
            Assert.Equal(new string[] { statusCode, "default" }, get.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.ListCustomer", get.OperationId);
            }
            else
            {
                Assert.Null(get.OperationId);
            }

            if (enablePagination)
            {
                Assert.True(get.Extensions.ContainsKey(Constants.xMsPageable));
            }
            else
            {
                Assert.False(get.Extensions.ContainsKey(Constants.xMsPageable));
            }
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForTopSupportedRestrictions(bool hasRestriction, bool supported)
        {
            // Arrange
            string annotation = String.Format(@"<Annotation Term=""Org.OData.Capabilities.V1.TopSupported"" Bool=""{0}"" />", supported);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, supported, "top");
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForSkipSupportedRestrictions(bool hasRestriction, bool supported)
        {
            // Arrange
            string annotation = String.Format(@"<Annotation Term=""Org.OData.Capabilities.V1.SkipSupported"" Bool=""{0}"" />", supported);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, supported, "skip");
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForSearchRestrictions(bool hasRestriction, bool searchable)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.SearchRestrictions"">
  <Record>
    <PropertyValue Property=""Searchable"" Bool=""{0}"" />
  </Record>
</Annotation>", searchable);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, searchable, "search");
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForFilterRestrictions(bool hasRestriction, bool filterable)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.FilterRestrictions"">
  <Record>
    <PropertyValue Property=""Filterable"" Bool=""{0}"" />
  </Record>
</Annotation>", filterable);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, filterable, "filter");
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForCountRestrictions(bool hasRestriction, bool countable)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.CountRestrictions"">
  <Record>
    <PropertyValue Property=""Countable"" Bool=""{0}"" />
  </Record>
</Annotation>", countable);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, countable, "count");
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForSortRestrictions(bool hasRestriction, bool sortable)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.SortRestrictions"">
  <Record>
    <PropertyValue Property=""Sortable"" Bool=""{0}"" />
  </Record>
</Annotation>", sortable);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, sortable, "$orderby", false);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        public void CreateEntitySetGetOperationReturnsParameterForExpandRestrictions(bool hasRestriction, bool expandable)
        {
            // Arrange
            string annotation = String.Format(@"
<Annotation Term=""Org.OData.Capabilities.V1.ExpandRestrictions"">
  <Record>
    <PropertyValue Property=""Expandable"" Bool=""{0}"" />
  </Record>
</Annotation>", expandable);

            // Act & Assert
            VerifyParameter(annotation, hasRestriction, expandable, "$expand", false);
        }

        [Theory]
        [InlineData(false, "Recursive")]
        [InlineData(false, "Single")]
        [InlineData(false, "None")]
        [InlineData(true, "Recursive")]
        [InlineData(true, "Single")]
        [InlineData(true, "None")]
        public void CreateEntitySetGetOperationReturnsParameterForNavigationRestrictions(bool hasRestriction, string navigability)
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
            VerifyParameter(annotation, hasRestriction, navigability == "None" ? false : true, "$select", false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntitySetGetOperationReturnsSecurityForReadRestrictions(bool enableAnnotation)
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
            IEdmModel model = GetEdmModel(enableAnnotation ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers));

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
  ]".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Empty(get.Security);
            }
        }

        public static IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <ComplexType Name=""Address"">
        <Property Name=""City"" Type=""Edm.String"" />
      </ComplexType>
      <EntityType Name=""Customer"">
        <Annotation Term=""Org.OData.Core.V1.Description"" String=""A business customer."" />
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""BillingAddress"" Type=""NS.Address"">
            <Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
                <Record>
                    <PropertyValue Property=""Description"" String=""Get the BillingAddress."" />            
                    <PropertyValue Property=""LongDescription"" String=""Get the BillingAddress value."" />            
                </Record>
            </Annotation>
            <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
                <Record>
                    <PropertyValue Property=""Description"" String=""Update the BillingAddress."" />            
                    <PropertyValue Property=""LongDescription"" String=""Update the BillingAddress value."" />            
                </Record>
            </Annotation>            
        </Property>
        <Property Name=""MailingAddress"" Type=""NS.Address"" Nullable=""false"" />
        <Property Name=""AlternativeAddresses"" Type=""Collection(NS.Address)"" Nullable=""false"">
            <Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
                <Record>
                    <PropertyValue Property=""Description"" String=""Get the AlternativeAddresses."" />            
                    <PropertyValue Property=""LongDescription"" String=""Get the AlternativeAddresses value."" />            
                </Record>
            </Annotation>
            <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
                <Record>
                    <PropertyValue Property=""Description"" String=""Update the AlternativeAddresses."" />            
                    <PropertyValue Property=""LongDescription"" String=""Update the AlternativeAddresses value."" />            
                </Record>
            </Annotation>        
        </Property>
      </EntityType>
      <EntityContainer Name =""Default"">
        <EntitySet Name=""Customers"" EntityType=""NS.Customer"">
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""Collection of business customers."" />
        </EntitySet>
      </EntityContainer>
      <Annotations Target=""NS.Default/Customers"">
        {0}
      </Annotations>
      <Annotations Target=""NS.Default/Customers"">
        <Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
          <Record>
            <PropertyValue Property=""Description"" String=""List customers."" />
            <PropertyValue Property=""LongDescription"" String=""Returns a list of customers."" />
            <PropertyValue Property=""ReadByKeyRestrictions"">
              <Record>
                <PropertyValue Property=""Description"" String=""Get customer."" />
                <PropertyValue Property=""LongDescription"" String=""Returns a single customer."" />
              </Record>
            </PropertyValue>
          </Record>
        </Annotation>
        <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
          <Record>
            <PropertyValue Property=""Description"" String=""Update customer."" />            
            <PropertyValue Property=""LongDescription"" String=""Updates a single customer."" />            
          </Record>
        </Annotation>
        <Annotation Term=""Org.OData.Capabilities.V1.DeleteRestrictions"">
          <Record>
            <PropertyValue Property=""Description"" String=""Delete customer."" />            
            <PropertyValue Property=""LongDescription"" String=""Deletes a single customer."" />            
          </Record>
        </Annotation>
      </Annotations>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            string modelText = string.Format(template, annotation);
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }

        private void VerifyParameter(string annotation, bool hasRestriction, bool supported, string queryOption, bool isReference = true)
        {
            // Arrange
            IEdmModel model = GetEdmModel(hasRestriction ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);

            Assert.NotNull(get.Parameters);
            if (!hasRestriction || supported)
            {
                Assert.Equal(8, get.Parameters.Count);
                if (isReference)
                {
                    Assert.Contains(queryOption, get.Parameters.Select(p => p.Reference?.Id));
                }
                else
                {
                    Assert.Contains(queryOption, get.Parameters.Select(p => p.Name));
                }
            }
            else
            {
                Assert.Equal(7, get.Parameters.Count);
                if (isReference)
                {
                    Assert.DoesNotContain(queryOption, get.Parameters.Select(p => p.Reference?.Id));
                }
                else
                {
                    Assert.DoesNotContain(queryOption, get.Parameters.Select(p => p.Name));
                }
            }
        }
    }
}
