// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiLinkGeneratorTests
    {
        [Fact]
        public void CreateLinksForSingleValuedNavigationProperties()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                ShowLinks = true,
                GenerateDerivedTypesProperties = false
            };
            ODataContext context = new(model, settings);
            IEdmSingleton admin = model.EntityContainer.FindSingleton("admin");
            Assert.NotNull(admin);

            IEdmEntityType adminEntityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "admin");
            IEdmNavigationProperty navProperty = adminEntityType.DeclaredNavigationProperties().First(c => c.Name == "serviceAnnouncement");
            ODataPath path = new(
                new ODataNavigationSourceSegment(admin),
                new ODataNavigationPropertySegment(navProperty));

            // Act
            IDictionary<string, OpenApiLink> links = context.CreateLinks(
                entityType: navProperty.ToEntityType(),
                entityName: adminEntityType.Name,
                entityKind: navProperty.PropertyKind.ToString(),
                path: path,
                navPropOperationId: "admin.ServiceAnnouncement");

            // Assert
            Assert.NotNull(links);
            Assert.Equal(3, links.Count);
            Assert.Collection(links,
                item =>
                {
                    Assert.Equal("healthOverviews", item.Key);
                    Assert.Equal("admin.ServiceAnnouncement.ListHealthOverviews", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("issues", item.Key);
                    Assert.Equal("admin.ServiceAnnouncement.ListIssues", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("messages", item.Key);
                    Assert.Equal("admin.ServiceAnnouncement.ListMessages", item.Value.OperationId);
                });
        }

        [Fact]
        public void CreateLinksForCollectionValuedNavigationProperties()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                ShowLinks = true,
                GenerateDerivedTypesProperties = false
            };
            ODataContext context = new(model, settings);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("admin");
            Assert.NotNull(singleton);

            IEdmEntityType singletonEntityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "admin");
            IEdmNavigationProperty navProperty = singletonEntityType.DeclaredNavigationProperties().First(c => c.Name == "serviceAnnouncement");
            IEdmNavigationProperty navProperty2 = navProperty.ToEntityType().DeclaredNavigationProperties().First(c => c.Name == "messages");
            ODataPath path = new(
                new ODataNavigationSourceSegment(singleton),
                new ODataNavigationPropertySegment(navProperty),
                new ODataNavigationPropertySegment(navProperty2));

            // Act
            IDictionary<string, OpenApiLink> links = context.CreateLinks(
                entityType: navProperty2.ToEntityType(),
                entityName: singletonEntityType.Name,
                entityKind: navProperty2.PropertyKind.ToString(),
                path: path);

            // Assert
            Assert.NotNull(links);
            Assert.Equal(6, links.Count);

            // Links will reference Operations and OperationImports
            Assert.Collection(links,
                item =>
                {
                    Assert.Equal("archive", item.Key);
                    Assert.Equal("admin.serviceAnnouncement.messages.archive", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("favorite", item.Key);
                    Assert.Equal("admin.serviceAnnouncement.messages.favorite", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("markRead", item.Key);
                    Assert.Equal("admin.serviceAnnouncement.messages.markRead", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("markUnread", item.Key);
                    Assert.Equal("admin.serviceAnnouncement.messages.markUnread", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("unarchive", item.Key);
                    Assert.Equal("admin.serviceAnnouncement.messages.unarchive", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("unfavorite", item.Key);
                    Assert.Equal("admin.serviceAnnouncement.messages.unfavorite", item.Value.OperationId);
                });
        }


        [Fact]
        public void CreateLinksForSingletons()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                ShowLinks = true,
                GenerateDerivedTypesProperties = false
            };
            ODataContext context = new(model, settings);
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("admin");
            Assert.NotNull(singleton);

            ODataPath path = new(
                new ODataNavigationSourceSegment(singleton));

            // Act
            IDictionary<string, OpenApiLink> links = context.CreateLinks(
                entityType: singleton.EntityType,
                entityName: singleton.Name,
                entityKind: singleton.ContainerElementKind.ToString(),
                path: path);

            // Assert
            Assert.NotNull(links);
            Assert.Equal(5, links.Count);
            Assert.Collection(links,
                item =>
                {
                    Assert.Equal("edge", item.Key);
                    Assert.Equal("admin.GetEdge", item.Value.OperationId);
                },
                item =>
                {
                Assert.Equal("sharepoint", item.Key);
                Assert.Equal("admin.GetSharepoint", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("serviceAnnouncement", item.Key);
                    Assert.Equal("admin.GetServiceAnnouncement", item.Value.OperationId);
                },
                item =>
                {
                Assert.Equal("reportSettings", item.Key);
                Assert.Equal("admin.GetReportSettings", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("windows", item.Key);
                    Assert.Equal("admin.GetWindows", item.Value.OperationId);
                });
        }

        [Fact]
        public void CreateLinksForEntities()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                ShowLinks = true,
                GenerateDerivedTypesProperties = false
            };
            ODataContext context = new(model, settings);
            IEdmEntitySet entityset = model.EntityContainer.FindEntitySet("agreements");
            Assert.NotNull(entityset);

            ODataPath path = new(
                new ODataNavigationSourceSegment(entityset),
                new ODataKeySegment(entityset.EntityType));

            var parameters = new List<OpenApiParameter>()
            {
                new OpenApiParameter()
                {
                    Name = "agreement-id",
                    In = ParameterLocation.Path,
                    Description = "key: id of agreement",
                    Required = true,
                    Schema = new OpenApiSchema()
                    {
                        Type = "string"
                    }
                }
            };

            // Act
            IDictionary<string, OpenApiLink> links = context.CreateLinks(
                entityType: entityset.EntityType,
                entityName: entityset.Name,
                entityKind: entityset.ContainerElementKind.ToString(),
                path: path,
                parameters: parameters);

            // Assert
            Assert.NotNull(links);
            Assert.Equal(3, links.Count);
            Assert.Collection(links,
                item =>
                {
                    Assert.Equal("acceptances", item.Key);
                    Assert.Equal("agreements.ListAcceptances", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("file", item.Key);
                    Assert.Equal("agreements.GetFile", item.Value.OperationId);
                },
                item =>
                {
                    Assert.Equal("files", item.Key);
                    Assert.Equal("agreements.ListFiles", item.Value.OperationId);
                });
        }
    }
}
