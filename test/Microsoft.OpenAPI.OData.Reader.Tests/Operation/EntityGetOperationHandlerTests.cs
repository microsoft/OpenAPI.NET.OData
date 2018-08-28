// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class EntityGetOperationHandlerTests
    {
        private EntityGetOperationHandler _operationHandler = new EntityGetOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEntityGetOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel("");
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                OperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet), new ODataKeySegment(entitySet.EntityType()));

            // Act
            var get = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(get);
            Assert.Equal("Get entity from Customers by key", get.Summary);
            Assert.NotNull(get.Tags);
            var tag = Assert.Single(get.Tags);
            Assert.Equal("Customers.Customer", tag.Name);

            Assert.NotNull(get.Parameters);
            Assert.Equal(3, get.Parameters.Count);
            Assert.Null(get.RequestBody);

            Assert.NotNull(get.Responses);
            Assert.Equal(2, get.Responses.Count);
            Assert.Equal(new[] { "200", "default" }, get.Responses.Select(r => r.Key));

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

        private void VerifyParameter(string annotation, bool hasRestriction, bool supported, string queryOption)
        {
            // Arrange
            IEdmModel model = EntitySetGetOperationHandlerTests.GetEdmModel(hasRestriction ? annotation : "");
            ODataContext context = new ODataContext(model);
            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            Assert.NotNull(customers); // guard
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(customers), new ODataKeySegment(customers.EntityType()));

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
