// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.PathItem.Tests;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class NavigationPropertyGetOperationHandlerTests
    {
        public NavigationPropertyGetOperationHandlerTests()
        {
          _openApiDocument.AddComponent("Delegated (work or school account)", new OpenApiSecurityScheme {
            Type = SecuritySchemeType.OAuth2,
          });
          _openApiDocument.AddComponent("Application", new OpenApiSecurityScheme {
            Type = SecuritySchemeType.OAuth2,
          });
        }
        private readonly OpenApiDocument _openApiDocument = new();
        private NavigationPropertyGetOperationHandler _operationHandler => new(_openApiDocument);

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
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

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
        public void CreateNavigationGetOperationWithTargetPathAnnotationsAndNavigationPropertyAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "Friends");
            ODataPath path = new(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("List friends", operation.Summary);
            Assert.Equal("List the friends of a specific person", operation.Description);
         
            Assert.NotNull(operation.ExternalDocs);
            Assert.Equal("Find more info here", operation.ExternalDocs.Description);
            Assert.Equal("https://learn.microsoft.com/graph/api/person-list-friends?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());

            Assert.NotNull(operation.Parameters);
            Assert.Equal(10, operation.Parameters.Count);
            Assert.Contains(operation.Parameters, x => x.Name == "ConsistencyLevel");
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
            _openApiDocument.Tags = context.CreateTags();

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
        public void CreateNavigationGetOperationWithUnresolvedFunctionTargetUsesPropertySortRestrictions(bool sortable)
        {
            VerifyNavigationSortRestrictions(viaFunction: true, propertySortable: sortable, pathSortable: null);
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void CreateNavigationGetOperationWithResolvedTargetUsesPathSortRestrictions(bool propertySortable, bool? pathSortable)
        {
            VerifyNavigationSortRestrictions(viaFunction: false, propertySortable, pathSortable);
        }

        private void VerifyNavigationSortRestrictions(bool viaFunction, bool propertySortable, bool? pathSortable)
        {
            string pathAnnotation = pathSortable.HasValue ? $@"
              <Annotations Target=""microsoft.graph.GraphService/tables/columns"">
                <Annotation Term=""Org.OData.Capabilities.V1.SortRestrictions"">
                  <Record>
                    <PropertyValue Property=""Sortable"" Bool=""{pathSortable.Value.ToString().ToLowerInvariant()}"" />
                  </Record>
                </Annotation>
              </Annotations>" : "";
            string csdl = $@"
              <edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
              <edmx:DataServices>
              <Schema Namespace=""microsoft.graph"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
                <EntityType Name=""workbookTable"">
                  <Key><PropertyRef Name=""id"" /></Key>
                  <Property Name=""id"" Type=""Edm.String"" Nullable=""false"" />
                  <NavigationProperty Name=""columns"" Type=""Collection(microsoft.graph.workbookTableColumn)"" ContainsTarget=""true"">
                    <Annotation Term=""Org.OData.Capabilities.V1.SortRestrictions"">
                      <Record>
                        <PropertyValue Property=""Sortable"" Bool=""{propertySortable.ToString().ToLowerInvariant()}"" />
                        <PropertyValue Property=""NonSortableProperties"">
                          <Collection><PropertyPath>id</PropertyPath></Collection>
                        </PropertyValue>
                      </Record>
                    </Annotation>
                  </NavigationProperty>
                </EntityType>
                <EntityType Name=""workbookTableColumn"">
                  <Key><PropertyRef Name=""id"" /></Key>
                  <Property Name=""id"" Type=""Edm.String"" Nullable=""false"" />
                  <Property Name=""name"" Type=""Edm.String"" />
                </EntityType>
                <Function Name=""itemAt"" IsBound=""true"" IsComposable=""true"">
                  <Parameter Name=""bindingParameter"" Type=""Collection(microsoft.graph.workbookTable)"" />
                  <Parameter Name=""index"" Type=""Edm.Int32"" Nullable=""false"" />
                  <ReturnType Type=""microsoft.graph.workbookTable"" />
                </Function>
                <EntityContainer Name=""GraphService"">
                  <EntitySet Name=""tables"" EntityType=""microsoft.graph.workbookTable"" />
                </EntityContainer>
                {pathAnnotation}
              </Schema>
              </edmx:DataServices>
              </edmx:Edmx>";
            IEdmModel model = CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            IEdmEntitySet tables = model.EntityContainer.FindEntitySet("tables");
            IEdmNavigationProperty columns = tables.EntityType.DeclaredNavigationProperties().Single();
            var path = new ODataPath(new ODataNavigationSourceSegment(tables));
            if (viaFunction)
            {
                path.Push(new ODataOperationSegment(model.SchemaElements.OfType<IEdmFunction>().Single()));
            }
            else
            {
                path.Push(new ODataKeySegment(tables.EntityType));
            }
            path.Push(new ODataNavigationPropertySegment(columns));

            var target = model.GetTargetPath(path.GetTargetPath(model));
            Assert.Equal(viaFunction, target is null);

            var settings = new OpenApiConvertSettings { UseStringArrayForQueryOptionsSchema = false };
            var operation = _operationHandler.CreateOperation(new ODataContext(model, settings), path);

            if (viaFunction ? propertySortable : pathSortable ?? true)
            {
                var orderBy = Assert.Single(operation.Parameters, parameter => parameter.Name == "$orderby");
                var expectedProperties = viaFunction
                    ? new[] { "name", "name desc" }
                    : new[] { "id", "id desc", "name", "name desc" };
                Assert.Equal(expectedProperties, orderBy.Schema.Items.Enum.Select(value => value.GetValue<string>()));
            }
            else
            {
                Assert.DoesNotContain(operation.Parameters, parameter => parameter.Name == "$orderby");
            }
        }

        [Fact]
        public void CreateNavigationGetOperationViaOverloadedComposableFunctionReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings()
            {
                EnableOperationId = true
            });

            IEdmEntitySet drives = model.EntityContainer.FindEntitySet("drives");
            IEdmEntityType drive = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "drive");
            IEdmNavigationProperty items = drive.DeclaredNavigationProperties().First(c => c.Name == "items");
            IEdmEntityType driveItem = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "driveItem");
            IEdmNavigationProperty workbook = driveItem.DeclaredNavigationProperties().First(c => c.Name == "workbook");
            IEdmEntityType workbookEntity = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "workbook");
            IEdmNavigationProperty worksheets = workbookEntity.DeclaredNavigationProperties().First(c => c.Name == "worksheets");
            IEdmEntityType workbookWorksheet = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "workbookWorksheet");
            IEdmOperation usedRangeWithParams = model.SchemaElements.OfType<IEdmOperation>().First(f => f.Name == "usedRange" && f.Parameters.Any(x => x.Name.Equals("valuesOnly")));
            IEdmOperation usedRange = model.SchemaElements.OfType<IEdmOperation>().First(f => f.Name == "usedRange" && f.Parameters.Count() == 1);
            IEdmEntityType workbookRange = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "workbookRange");
            IEdmNavigationProperty format = workbookRange.DeclaredNavigationProperties().First(c => c.Name == "format");


            ODataPath path1 = new(new ODataNavigationSourceSegment(drives),
                new ODataKeySegment(drive),
                new ODataNavigationPropertySegment(items),
                new ODataKeySegment(driveItem),
                new ODataNavigationPropertySegment(workbook),
                new ODataNavigationPropertySegment(worksheets),
                new ODataKeySegment(workbookWorksheet),
                new ODataOperationSegment(usedRangeWithParams),
                new ODataNavigationPropertySegment(format));

            ODataPath path2 = new(new ODataNavigationSourceSegment(drives),
                new ODataKeySegment(drive),
                new ODataNavigationPropertySegment(items),
                new ODataKeySegment(driveItem),
                new ODataNavigationPropertySegment(workbook),
                new ODataNavigationPropertySegment(worksheets),
                new ODataKeySegment(workbookWorksheet),
                new ODataOperationSegment(usedRange),
                new ODataNavigationPropertySegment(format));

            // Act
            var operation1 = _operationHandler.CreateOperation(context, path1);
            var operation2 = _operationHandler.CreateOperation(context, path2);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation1);
            Assert.NotNull(operation2);

            Assert.Equal("drives.items.workbook.worksheets.usedRange.GetFormat-206d", operation1.OperationId);
            Assert.Equal("drives.items.workbook.worksheets.usedRange.GetFormat-ec2c", operation2.OperationId);

            Assert.NotNull(operation1.Parameters);
            Assert.Equal(6, operation1.Parameters.Count);
            Assert.Contains(operation1.Parameters, x => x.Name == "valuesOnly");

            Assert.NotNull(operation2.Parameters);
            Assert.Equal(5, operation2.Parameters.Count);
            Assert.DoesNotContain(operation2.Parameters, x => x.Name == "valuesOnly");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateNavigationGetOperationReturnsSecurityForReadRestrictions(bool enableAnnotation)
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
            IEdmEntityType entityType = entitySet.EntityType;

            IEdmNavigationProperty property = entityType.DeclaredNavigationProperties().FirstOrDefault(c => c.Name == "Orders");
            Assert.NotNull(property);

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(entitySet),
                new ODataKeySegment(entityType),
                new ODataNavigationPropertySegment(property));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation);

            if (enableAnnotation)
            {
              Assert.NotNull(operation.Security);
                Assert.Equal(2, operation.Security.Count);

                string json = await operation.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
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
                Assert.Null(operation.Security);
            }
        }

        [Fact]
        public void CreateNavigationGetOperationWithAlternateKeyReturnsCorrectOperationId()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings()
            {
                EnableOperationId = true
            });

            IEdmSingleton singleton = model.EntityContainer.FindSingleton("communications");
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "cloudCommunications");
            IEdmNavigationProperty navProp = entityType.DeclaredNavigationProperties().First(c => c.Name == "onlineMeetings");
            IDictionary<string, string> keyMappings = new Dictionary<string, string> { { "joinWebUrl", "joinWebUrl" } };

            ODataPath path = new(new ODataNavigationSourceSegment(singleton),
                new ODataNavigationPropertySegment(navProp),
                new ODataKeySegment(entityType, keyMappings)
                {
                    IsAlternateKey = true
                });

            // Act
            var operation = _operationHandler.CreateOperation(context, path);
            _openApiDocument.Tags = context.CreateTags();

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("communications.onlineMeetings.GetByJoinWebUrl", operation.OperationId);
        }
    }
}
