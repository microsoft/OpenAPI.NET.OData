// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Microsoft.OpenApi.OData.Edm;
using System.Linq;
using Xunit;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Any;

namespace Microsoft.OpenApi.OData.PathItem.Tests
{
    public class ODataTypeCastPathItemHandlerTests
    {
        private readonly ODataTypeCastPathItemHandler _pathItemHandler = new();

        [Fact]
        public void CreateODataTypeCastPathItemAddsCustomAttributeValuesToPathExtensions()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping = new()
            {
                {
                    "ags:IsHidden",
                    "x-ms-isHidden"
                },
                {
                    "WorkloadName",
                    "x-ms-workloadName"
                }
            };
            
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmEntityType employee = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Employee");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Friends");
            ODataPath path = new(new ODataNavigationSourceSegment(people),
                                                                    new ODataKeySegment(people.EntityType),
                                                                    new ODataNavigationPropertySegment(navProperty),
                                                                    new ODataTypeCastSegment(employee, model));

            // Act
            var pathItem = _pathItemHandler.CreatePathItem(context, path);

            // Assert
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Extensions);

            pathItem.Extensions.TryGetValue("x-ms-isHidden", out IOpenApiExtension isHiddenExtension);
            string isHiddenValue = (isHiddenExtension as OpenApiString)?.Value;
            Assert.Equal("true", isHiddenValue);

            pathItem.Extensions.TryGetValue("x-ms-workloadName", out IOpenApiExtension isOwnerExtension);
            string isOwnerValue = (isOwnerExtension as OpenApiString)?.Value;
            Assert.Equal("People", isOwnerValue);
        }
    }
}
