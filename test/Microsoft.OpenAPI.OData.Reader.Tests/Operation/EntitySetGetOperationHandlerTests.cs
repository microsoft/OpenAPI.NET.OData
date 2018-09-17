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
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntitySetGetOperationHandlerTests
    {
        private EntitySetGetOperationHandler _operationHandler = new EntitySetGetOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntitySetGetOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get entities from " + entitySet.Name, get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(8, get.Parameters.Count);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);

            if (enableOperationId)
            {
                Assert.Equal("Customers.Customer.ListCustomer", get.OperationId);
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
         <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
      </EntityContainer>
      <Annotations Target=""NS.Default/Customers"">
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
