// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiSchemaGeneratorTest
    {
        private readonly ITestOutputHelper _output;
        public OpenApiSchemaGeneratorTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CreateSchemasThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;
            OpenApiDocument openApiDocument = new();

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.AddSchemasToDocument(openApiDocument));
        }

        [Theory]
        [InlineData(true, false, "BaseCollectionPaginationCountResponse")]
        [InlineData(false, true, "BaseCollectionPaginationCountResponse")]
        [InlineData(true, true, "BaseCollectionPaginationCountResponse")]
        [InlineData(false, false)]
        public void CreatesCollectionResponseSchema(bool enablePagination, bool enableCount, string referenceId = null)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiDocument openApiDocument = new();
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = enablePagination,
                EnableCount = enableCount
            };
            ODataContext context = new(model, settings);

            // Act & Assert
            context.AddSchemasToDocument(openApiDocument);

            var stringCollectionResponse = openApiDocument.Components.Schemas["StringCollectionResponse"];
            var flightCollectionResponse = openApiDocument.Components.Schemas["Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse"];

            if (enablePagination || enableCount)
            {
                Assert.Collection(stringCollectionResponse.AllOf,
                item =>
                {
                    var itemReference = Assert.IsType<OpenApiSchemaReference>(item);
                    Assert.Equal(referenceId, itemReference.Reference.Id);
                },
                item =>
                {
                    Assert.Equal(JsonSchemaType.Array, item.Properties["value"].Type);
                });

                Assert.Single(flightCollectionResponse.AllOf?.Where(x => x.Properties.TryGetValue("value", out var valueProp) && 
                                                                (valueProp.Type & JsonSchemaType.Array) is JsonSchemaType.Array &&
                                                                valueProp.Items is OpenApiSchemaReference openApiSchemaReference &&
                                                                "Microsoft.OData.Service.Sample.TrippinInMemory.Models.Flight".Equals(openApiSchemaReference.Reference.Id)));
            }
            else
            {
                Assert.Equal(JsonSchemaType.Array, stringCollectionResponse.Properties["value"].Type);
                Assert.Equal(JsonSchemaType.Array, flightCollectionResponse.Properties["value"].Type);
                var itemsReference = Assert.IsType<OpenApiSchemaReference>(flightCollectionResponse.Properties["value"].Items);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Flight", itemsReference.Reference.Id);
            }
        }

        [Fact]
        public void CreatesRefRequestBodySchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiDocument openApiDocument = new();
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = true,
            };
            ODataContext context = new(model, settings);

            // Act & Assert
            context.AddSchemasToDocument(openApiDocument);

            openApiDocument.Components.Schemas.TryGetValue(Constants.ReferenceCreateSchemaName, out var refRequestBody);

            Assert.NotNull(refRequestBody);
            Assert.Equal(JsonSchemaType.Object, refRequestBody.Type);
            Assert.Equal(Constants.OdataId, refRequestBody.Properties.First().Key);
            Assert.Equal(JsonSchemaType.String, refRequestBody.Properties.First().Value.Type);
            Assert.Equal(JsonSchemaType.Object, refRequestBody.AdditionalProperties.Type);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatesRefOdataAnnotationResponseSchemas(bool enableOdataAnnotationRef)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiDocument openApiDocument = new();
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = true,
                EnableCount = true,
                EnableODataAnnotationReferencesForResponses = enableOdataAnnotationRef
            };
            ODataContext context = new(model, settings);

            // Act
            context.AddSchemasToDocument(openApiDocument);

            // Assert
            Assert.NotNull(openApiDocument.Components.Schemas);
            Assert.NotEmpty(openApiDocument.Components.Schemas);
            openApiDocument.Components.Schemas.TryGetValue(Constants.BaseCollectionPaginationCountResponse, out var refPaginationCount);
            openApiDocument.Components.Schemas.TryGetValue(Constants.BaseDeltaFunctionResponse, out var refDeltaFunc);
            if (enableOdataAnnotationRef)
            {
                Assert.NotNull(refPaginationCount);
                Assert.NotNull(refDeltaFunc);
                Assert.True(refPaginationCount.Properties.ContainsKey("@odata.nextLink"));
                Assert.True(refPaginationCount.Properties.ContainsKey("@odata.count"));
                Assert.True(refDeltaFunc.Properties.ContainsKey("@odata.nextLink"));
                Assert.True(refDeltaFunc.Properties.ContainsKey("@odata.deltaLink"));
            }
            else
            {
                Assert.Null(refPaginationCount);
                Assert.Null(refDeltaFunc);
            }
        }

        #region StructuredTypeSchema
        [Fact]
        public void CreateStructuredTypeSchemaThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateStructuredTypeSchema(structuredType: null, new()));
        }

        [Fact]
        public void CreateStructuredTypeSchemaThrowArgumentNullEnumType()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("structuredType", () => context.CreateStructuredTypeSchema(structuredType: null, new()));
        }

        [Fact]
        public async Task CreateStructuredTypeSchemaForEntityTypeWithDiscriminatorValueEnabledReturnsCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings
            {
                EnableDiscriminatorValue = true,
            });

            string derivedType = "user";
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "directoryObject");
            IEdmEntityType derivedEntity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == derivedType);
            Assert.NotNull(entity);
            Assert.NotNull(derivedEntity);

            // Act
            var schema = context.CreateStructuredTypeSchema(entity, new());
            var derivedSchema = context.CreateStructuredTypeSchema(derivedEntity, new());
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // Assert
            Assert.True(derivedSchema.AllOf.FirstOrDefault(x => derivedType.Equals(x.Title))?.Properties.ContainsKey("@odata.type"));
            Assert.NotNull(json);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""allOf"": [
    {
      ""$ref"": ""#/components/schemas/microsoft.graph.entity""
    },
    {
      ""title"": ""directoryObject"",
      ""required"": [
        ""@odata.type""
      ],
      ""type"": ""object"",
      ""properties"": {
        ""deletedDateTime"": {
          ""pattern"": ""^[0-9]{4,}-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]([.][0-9]{1,12})?(Z|[+-][0-9][0-9]:[0-9][0-9])$"",
          ""type"": ""string"",
          ""description"": ""Date and time when this object was deleted. Always null when the object hasn't been deleted."",
          ""format"": ""date-time"",
          ""nullable"": true
        },
        ""@odata.type"": {
          ""type"": ""string"",
          ""default"": ""#microsoft.graph.directoryObject""
        }
      },
      ""discriminator"": {
        ""propertyName"": ""@odata.type"",
        ""mapping"": {
          ""#microsoft.graph.user"": ""#/components/schemas/microsoft.graph.user"",
          ""#microsoft.graph.servicePrincipal"": ""#/components/schemas/microsoft.graph.servicePrincipal"",
          ""#microsoft.graph.group"": ""#/components/schemas/microsoft.graph.group"",
          ""#microsoft.graph.device"": ""#/components/schemas/microsoft.graph.device"",
          ""#microsoft.graph.administrativeUnit"": ""#/components/schemas/microsoft.graph.administrativeUnit"",
          ""#microsoft.graph.application"": ""#/components/schemas/microsoft.graph.application"",
          ""#microsoft.graph.policyBase"": ""#/components/schemas/microsoft.graph.policyBase"",
          ""#microsoft.graph.appManagementPolicy"": ""#/components/schemas/microsoft.graph.appManagementPolicy"",
          ""#microsoft.graph.stsPolicy"": ""#/components/schemas/microsoft.graph.stsPolicy"",
          ""#microsoft.graph.homeRealmDiscoveryPolicy"": ""#/components/schemas/microsoft.graph.homeRealmDiscoveryPolicy"",
          ""#microsoft.graph.tokenIssuancePolicy"": ""#/components/schemas/microsoft.graph.tokenIssuancePolicy"",
          ""#microsoft.graph.tokenLifetimePolicy"": ""#/components/schemas/microsoft.graph.tokenLifetimePolicy"",
          ""#microsoft.graph.claimsMappingPolicy"": ""#/components/schemas/microsoft.graph.claimsMappingPolicy"",
          ""#microsoft.graph.activityBasedTimeoutPolicy"": ""#/components/schemas/microsoft.graph.activityBasedTimeoutPolicy"",
          ""#microsoft.graph.authorizationPolicy"": ""#/components/schemas/microsoft.graph.authorizationPolicy"",
          ""#microsoft.graph.tenantRelationshipAccessPolicyBase"": ""#/components/schemas/microsoft.graph.tenantRelationshipAccessPolicyBase"",
          ""#microsoft.graph.crossTenantAccessPolicy"": ""#/components/schemas/microsoft.graph.crossTenantAccessPolicy"",
          ""#microsoft.graph.tenantAppManagementPolicy"": ""#/components/schemas/microsoft.graph.tenantAppManagementPolicy"",
          ""#microsoft.graph.externalIdentitiesPolicy"": ""#/components/schemas/microsoft.graph.externalIdentitiesPolicy"",
          ""#microsoft.graph.permissionGrantPolicy"": ""#/components/schemas/microsoft.graph.permissionGrantPolicy"",
          ""#microsoft.graph.servicePrincipalCreationPolicy"": ""#/components/schemas/microsoft.graph.servicePrincipalCreationPolicy"",
          ""#microsoft.graph.identitySecurityDefaultsEnforcementPolicy"": ""#/components/schemas/microsoft.graph.identitySecurityDefaultsEnforcementPolicy"",
          ""#microsoft.graph.extensionProperty"": ""#/components/schemas/microsoft.graph.extensionProperty"",
          ""#microsoft.graph.endpoint"": ""#/components/schemas/microsoft.graph.endpoint"",
          ""#microsoft.graph.resourceSpecificPermissionGrant"": ""#/components/schemas/microsoft.graph.resourceSpecificPermissionGrant"",
          ""#microsoft.graph.contract"": ""#/components/schemas/microsoft.graph.contract"",
          ""#microsoft.graph.directoryObjectPartnerReference"": ""#/components/schemas/microsoft.graph.directoryObjectPartnerReference"",
          ""#microsoft.graph.directoryRole"": ""#/components/schemas/microsoft.graph.directoryRole"",
          ""#microsoft.graph.directoryRoleTemplate"": ""#/components/schemas/microsoft.graph.directoryRoleTemplate"",
          ""#microsoft.graph.directorySettingTemplate"": ""#/components/schemas/microsoft.graph.directorySettingTemplate"",
          ""#microsoft.graph.organization"": ""#/components/schemas/microsoft.graph.organization"",
          ""#microsoft.graph.orgContact"": ""#/components/schemas/microsoft.graph.orgContact""
        }
      }
    }
  ]
}"), json));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateStructuredTypeSchemaForComplexTypeWithDiscriminatorValueEnabledReturnsCorrectSchema(bool enableTypeDisambiguationForOdataTypePropertyDefaultValue)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings
            {
                EnableDiscriminatorValue = true,
                EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty = enableTypeDisambiguationForOdataTypePropertyDefaultValue
            });

            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(t => t.Name == "userSet");
            Assert.NotNull(complex); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(complex, new());
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            string expected = enableTypeDisambiguationForOdataTypePropertyDefaultValue ?
                @"{
  ""title"": ""userSet"",
  ""required"": [
    ""@odata.type""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""isBackup"": {
      ""type"": ""boolean"",
      ""description"": ""For a user in an approval stage, this property indicates whether the user is a backup fallback approver."",
      ""nullable"": true
    },
    ""@odata.type"": {
      ""type"": ""string""
    }
  },
  ""discriminator"": {
    ""propertyName"": ""@odata.type"",
    ""mapping"": {
      ""#microsoft.graph.connectedOrganizationMembers"": ""#/components/schemas/microsoft.graph.connectedOrganizationMembers"",
      ""#microsoft.graph.externalSponsors"": ""#/components/schemas/microsoft.graph.externalSponsors"",
      ""#microsoft.graph.groupMembers"": ""#/components/schemas/microsoft.graph.groupMembers"",
      ""#microsoft.graph.internalSponsors"": ""#/components/schemas/microsoft.graph.internalSponsors"",
      ""#microsoft.graph.requestorManager"": ""#/components/schemas/microsoft.graph.requestorManager"",
      ""#microsoft.graph.singleUser"": ""#/components/schemas/microsoft.graph.singleUser""
    }
  }
}"
:
                @"{
  ""title"": ""userSet"",
  ""required"": [
    ""@odata.type""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""isBackup"": {
      ""type"": ""boolean"",
      ""description"": ""For a user in an approval stage, this property indicates whether the user is a backup fallback approver."",
      ""nullable"": true
    },
    ""@odata.type"": {
      ""type"": ""string"",
      ""default"": ""#microsoft.graph.userSet""
    }
  },
  ""discriminator"": {
    ""propertyName"": ""@odata.type"",
    ""mapping"": {
      ""#microsoft.graph.connectedOrganizationMembers"": ""#/components/schemas/microsoft.graph.connectedOrganizationMembers"",
      ""#microsoft.graph.externalSponsors"": ""#/components/schemas/microsoft.graph.externalSponsors"",
      ""#microsoft.graph.groupMembers"": ""#/components/schemas/microsoft.graph.groupMembers"",
      ""#microsoft.graph.internalSponsors"": ""#/components/schemas/microsoft.graph.internalSponsors"",
      ""#microsoft.graph.requestorManager"": ""#/components/schemas/microsoft.graph.requestorManager"",
      ""#microsoft.graph.singleUser"": ""#/components/schemas/microsoft.graph.singleUser""
    }
  }
}";

            Assert.True(JsonObject.DeepEquals(JsonObject.Parse(expected), JsonObject.Parse(json)));
        }

        [Fact]
        public async Task CreateStructuredTypePropertiesSchemaWithCustomAttributeReturnsCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping.Add("IsHidden", "x-ms-isHidden");
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "userSettings");
            Assert.NotNull(entity); // Guard

            // Act
            OpenApiSchema schema = context.CreateStructuredTypeSchema(entity, new());
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.True(JsonObject.DeepEquals(JsonObject.Parse(@"{
  ""allOf"": [
    {
      ""$ref"": ""#/components/schemas/microsoft.graph.entity""
    },
    {
      ""title"": ""userSettings"",
      ""type"": ""object"",
      ""properties"": {
        ""contributionToContentDiscoveryAsOrganizationDisabled"": {
          ""type"": ""boolean"",
          ""description"": ""Reflects the Office Delve organization level setting. When set to true, the organization doesn't have access to Office Delve. This setting is read-only and can only be changed by administrators in the SharePoint admin center.""
        },
        ""contributionToContentDiscoveryDisabled"": {
          ""type"": ""boolean"",
          ""description"": ""When set to true, documents in the user's Office Delve are disabled. Users can control this setting in Office Delve.""
        },
        ""itemInsights"": {
          ""anyOf"": [
            {
              ""$ref"": ""#/components/schemas/microsoft.graph.userInsightsSettings""
            },
            {
              ""type"": ""object"",
              ""nullable"": true
            }
          ],
          ""description"": ""The user's settings for the visibility of meeting hour insights, and insights derived between a user and other items in Microsoft 365, such as documents or sites. Get userInsightsSettings through this navigation property."",
          ""x-ms-navigationProperty"": true
        },
        ""contactMergeSuggestions"": {
          ""anyOf"": [
            {
              ""$ref"": ""#/components/schemas/microsoft.graph.contactMergeSuggestions""
            },
            {
              ""type"": ""object"",
              ""nullable"": true
            }
          ],
          ""description"": ""The user's settings for the visibility of merge suggestion for the duplicate contacts in the user's contact list."",
          ""x-ms-navigationProperty"": true
        },
        ""regionalAndLanguageSettings"": {
          ""anyOf"": [
            {
              ""$ref"": ""#/components/schemas/microsoft.graph.regionalAndLanguageSettings""
            },
            {
              ""type"": ""object"",
              ""nullable"": true
            }
          ],
          ""description"": ""The user's preferences for languages, regional locale and date/time formatting."",
          ""x-ms-navigationProperty"": true
        },
        ""shiftPreferences"": {
          ""anyOf"": [
            {
              ""$ref"": ""#/components/schemas/microsoft.graph.shiftPreferences""
            },
            {
              ""type"": ""object"",
              ""nullable"": true
            }
          ],
          ""description"": ""The shift preferences for the user."",
          ""x-ms-navigationProperty"": true
        }
      }
    }
  ]
}"), JsonObject.Parse(json)));
        }

        [Fact]
        public async Task CreateComplexTypeWithoutBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model, new OpenApiConvertSettings
            {
                ShowSchemaExamples = true
            });
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(t => t.Name == "Address");
            Assert.NotNull(complex); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(complex, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(JsonSchemaType.Object, schema.Type);
            Assert.Null(schema.AllOf);

            Assert.NotNull(schema.Properties);
            Assert.Equal(2, schema.Properties.Count);
            Assert.Equal(new string[] { "Street", "City" }, schema.Properties.Select(e => e.Key));
            Assert.Equal("Complex type 'Address' description.", schema.Description);
            Assert.Equal("Address", schema.Title);

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.True(JsonObject.DeepEquals(JsonObject.Parse(@"{
  ""title"": ""Address"",
  ""type"": ""object"",
  ""properties"": {
    ""Street"": {
      ""type"": ""string"",
      ""nullable"": true
    },
    ""City"": {
      ""type"": ""string"",
      ""nullable"": true
    }
  },
  ""description"": ""Complex type 'Address' description."",
  ""example"": {
    ""Street"": ""string"",
    ""City"": ""string""
  }
}"), JsonObject.Parse(json)));
        }

        [Fact]
        public async Task CreateComplexTypeWithBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model, new OpenApiConvertSettings
            {
                IEEE754Compatible = true,
                ShowSchemaExamples = true
            });
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(t => t.Name == "Tree");
            Assert.NotNull(complex); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(complex, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type.ToIdentifiers());

            Assert.NotNull(schema.AllOf);
            Assert.Null(schema.AnyOf);
            Assert.Null(schema.OneOf);
            Assert.Null(schema.Properties);

            Assert.Equal(2, schema.AllOf.Count);
            var baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.AllOf.First());
            Assert.NotNull(baseSchema.Reference);
            Assert.Equal(ReferenceType.Schema, baseSchema.Reference.Type);
            Assert.Equal("NS.LandPlant", baseSchema.Reference.Id);

            var declaredSchema = schema.AllOf.Last();
            Assert.Equal(JsonSchemaType.Object, declaredSchema.Type);
            Assert.Null(declaredSchema.AllOf);
            Assert.Null(declaredSchema.AnyOf);
            Assert.Null(declaredSchema.OneOf);

            Assert.NotNull(declaredSchema.Properties);
            Assert.Single(declaredSchema.Properties);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Price", property.Key);
            Assert.Equal("decimal", property.Value.OneOf.FirstOrDefault(x => !string.IsNullOrEmpty(x.Format))?.Format);
            Assert.NotNull(property.Value.OneOf);
            Assert.Equal([JsonSchemaType.Number | JsonSchemaType.Null, JsonSchemaType.String | JsonSchemaType.Null], property.Value.OneOf.Select(e => e.Type));

            Assert.Equal("Complex type 'Tree' description.", declaredSchema.Description);
            Assert.Equal("Tree", declaredSchema.Title);

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""allOf"": [
    {
      ""$ref"": ""#/components/schemas/NS.LandPlant""
    },
    {
      ""title"": ""Tree"",
      ""type"": ""object"",
      ""properties"": {
        ""Price"": {
          ""oneOf"": [
            {
              ""type"": ""number"",
              ""format"": ""decimal"",
              ""nullable"": true
            },
            {
              ""type"": ""string"",
              ""nullable"": true
            }
          ]
        }
      },
      ""description"": ""Complex type 'Tree' description.""
    }
  ],
  ""example"": {
    ""Color"": ""Blue"",
    ""Continent"": ""Asia"",
    ""Name"": ""string"",
    ""Price"": 0
  }
}"), JsonNode.Parse(json)));
        }

        [Fact]
        public async Task CreateEntityTypeWithoutBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model, new OpenApiConvertSettings
            {
                ShowSchemaExamples = true
            });
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "Zoo");
            Assert.NotNull(entity); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(entity, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(JsonSchemaType.Object, schema.Type);
            Assert.Null(schema.AllOf);

            Assert.NotNull(schema.Properties);
            Assert.Equal(2, schema.Properties.Count);
            Assert.Equal(new string[] { "Id", "Creatures" }, schema.Properties.Select(e => e.Key));
            Assert.Equal("Entity type 'Zoo' description.", schema.Description);
            Assert.Equal("Zoo", schema.Title);

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""title"": ""Zoo"",
  ""type"": ""object"",
  ""properties"": {
    ""Id"": {
      ""maximum"": 2147483647,
      ""minimum"": -2147483648,
      ""type"": ""number"",
      ""format"": ""int32""
    },
    ""Creatures"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/components/schemas/NS.Creature""
      },
      ""x-ms-navigationProperty"": true
    }
  },
  ""description"": ""Entity type 'Zoo' description."",
  ""example"": {
    ""Id"": 0,
    ""Creatures"": [
      {
        ""@odata.type"": ""NS.Creature""
      }
    ]
  }
}"), JsonNode.Parse(json)));
        }

        [Fact]
        public async Task CreateEntityTypeWithBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model, new OpenApiConvertSettings
            {
                ShowSchemaExamples = true
            });
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "Human");
            Assert.NotNull(entity); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(entity, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type.ToIdentifiers());

            Assert.NotNull(schema.AllOf);
            Assert.Null(schema.AnyOf);
            Assert.Null(schema.OneOf);
            Assert.Null(schema.Properties);

            Assert.Equal(2, schema.AllOf.Count);
            var baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.AllOf.First());
            Assert.NotNull(baseSchema.Reference);
            Assert.Equal(ReferenceType.Schema, baseSchema.Reference.Type);
            Assert.Equal("NS.Animal", baseSchema.Reference.Id);

            var declaredSchema = schema.AllOf.Last();
            Assert.Equal(JsonSchemaType.Object, declaredSchema.Type);
            Assert.Null(declaredSchema.AllOf);
            Assert.Null(declaredSchema.AnyOf);
            Assert.Null(declaredSchema.OneOf);

            Assert.NotNull(declaredSchema.Properties);
            Assert.Single(declaredSchema.Properties);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Name", property.Key);
            Assert.Equal(JsonSchemaType.String, property.Value.Type);
            Assert.Null(property.Value.OneOf);

            Assert.Equal("Entity type 'Human' description.", declaredSchema.Description);
            Assert.Equal("Human", declaredSchema.Title);

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
            _output.WriteLine(json);
            // Assert
            Assert.NotNull(json);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""allOf"": [
    {
      ""$ref"": ""#/components/schemas/NS.Animal""
    },
    {
      ""title"": ""Human"",
      ""type"": ""object"",
      ""properties"": {
        ""Name"": {
          ""type"": ""string""
        }
      },
      ""description"": ""Entity type 'Human' description.""
    }
  ],
  ""example"": {
    ""Id"": 0,
    ""Age"": 0,
    ""Name"": ""string""
  }
}"), JsonNode.Parse(json)));
        }

        [Fact]
        public void CreateEntityTypeWithCrossReferenceBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.InheritanceEdmModelAcrossReferences;
            ODataContext context = new ODataContext(model, new OpenApiConvertSettings
            {
                ShowSchemaExamples = true
            });
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "Customer");
            Assert.NotNull(entity); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(entity, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type.ToIdentifiers());

            Assert.NotNull(schema.AllOf);
            Assert.Null(schema.AnyOf);
            Assert.Null(schema.OneOf);
            Assert.Null(schema.Properties);

            Assert.Equal(2, schema.AllOf.Count);
            var baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.AllOf.First());
            Assert.NotNull(baseSchema.Reference);
            Assert.Equal(ReferenceType.Schema, baseSchema.Reference.Type);
            Assert.Equal("SubNS.CustomerBase", baseSchema.Reference.Id);

            var declaredSchema = schema.AllOf.Last();
            Assert.Equal(JsonSchemaType.Object, declaredSchema.Type);
            Assert.Null(declaredSchema.AllOf);
            Assert.Null(declaredSchema.AnyOf);
            Assert.Null(declaredSchema.OneOf);

            Assert.NotNull(declaredSchema.Properties);
            Assert.Single(declaredSchema.Properties);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Extra", property.Key);
            Assert.Equal(JsonSchemaType.Number, property.Value.Type);
            Assert.Null(property.Value.OneOf);

            Assert.Equal("Customer", declaredSchema.Title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateStructuredTypeSchemaForEntityTypeWithDefaultValueForOdataTypePropertyEnabledOrDisabledReturnsCorrectSchema(bool enableTypeDisambiguationForOdataTypePropertyDefaultValue)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model, new OpenApiConvertSettings
            {
                EnableDiscriminatorValue = true,
                EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty = enableTypeDisambiguationForOdataTypePropertyDefaultValue
            });

            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "event");
            Assert.NotNull(entityType); // Guard

            // Act
            var schema = context.CreateStructuredTypeSchema(entityType, new());
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"default\": \"#microsoft.graph.event\"", json);
        }

        #endregion

        #region EnumTypeSchema
        [Fact]
        public void CreateEnumTypeSchemaThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEnumTypeSchema(enumType: null));
        }

        [Fact]
        public void CreateEnumTypeSchemaThrowArgumentNullEnumType()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("enumType", () => context.CreateEnumTypeSchema(enumType: null));
        }

        [Fact]
        public async Task CreateEnumTypeSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(t => t.Name == "Color");
            Assert.NotNull(enumType); // Guard

            // Act
            var schema = context.CreateEnumTypeSchema(enumType);

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(JsonSchemaType.String, schema.Type);
            Assert.Equal("Enum type 'Color' description.", schema.Description);
            Assert.Equal("Color", schema.Title);

            Assert.NotNull(schema.Enum);
            Assert.Equal(2, schema.Enum.Count);
            Assert.Equal([ "Blue", "White" ], schema.Enum.Select(e => e.ToString()));

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""title"": ""Color"",
  ""enum"": [
    ""Blue"",
    ""White""
  ],
  ""type"": ""string"",
  ""description"": ""Enum type 'Color' description.""
}"), JsonNode.Parse(json)));
        }
        #endregion

        #region EdmPropertySchema
        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public async Task CreatePropertySchemaForNonNullableEnumPropertyReturnSchema(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(e => e.Name == "Color");
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmProperty property = new EdmStructuralProperty(entitType, "ColorEnumValue", new EdmEnumTypeReference(enumType, false));

            // Act
            var schema = context.CreatePropertySchema(property, new());
            Assert.NotNull(schema);
            string json = await schema.SerializeAsJsonAsync(specVersion);

            // Assert

            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""$ref"": ""#/definitions/DefaultNs.Color""
}"), JsonNode.Parse(json)));
            }
            else
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""$ref"": ""#/components/schemas/DefaultNs.Color""
}"), JsonNode.Parse(json)));
            }
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi3_1)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        public async Task CreatePropertySchemaForNullableEnumPropertyReturnSchema(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(e => e.Name == "Color");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmProperty property = new EdmStructuralProperty(entityType, "ColorEnumValue", new EdmEnumTypeReference(enumType, true), "yellow");

            // Act
            var schema = context.CreatePropertySchema(property, new());
            Assert.NotNull(schema);
            string json = await schema.SerializeAsJsonAsync(specVersion);
            _output.WriteLine(json);

            var expected = JsonNode.Parse(specVersion switch {
                OpenApiSpecVersion.OpenApi2_0 =>
                """
                {
                  "$ref": "#/definitions/DefaultNs.Color"
                }
                """,
                OpenApiSpecVersion.OpenApi3_0 =>
                """
                {
                  "anyOf": [
                    {
                      "$ref": "#/components/schemas/DefaultNs.Color"
                    },
                    {
                      "type": "object",
                      "nullable": true
                    }
                  ],
                  "default": "yellow"
                }
                """,
                OpenApiSpecVersion.OpenApi3_1 =>
                """
                {
                  "anyOf": [
                    {
                      "$ref": "#/components/schemas/DefaultNs.Color"
                    },
                    {
                      "type": "null"
                    }
                  ],
                  "default": "yellow"
                }
                """,
                _ => throw new NotSupportedException()
            });

            Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(json)));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        public async Task CreatePropertySchemaWithComputedAnnotationReturnsCorrectSchema(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "bookingAppointment");
            IEdmProperty property = entityType.Properties().FirstOrDefault(x => x.Name == "duration");

            // Act
            var schema = context.CreatePropertySchema(property, new());
            Assert.NotNull(schema);
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(specVersion));

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""format"": ""duration"",
  ""description"": ""The length of the appointment, denoted in ISO8601 format."",
  ""pattern"": ""^-?P([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+([.][0-9]+)?S)?)?$"",
  ""type"": ""string""
}"), json)); // TODO FIXME after resolution of https://github.com/microsoft/OpenAPI.NET/issues/2272
            }
            else
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""pattern"": ""^-?P([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+([.][0-9]+)?S)?)?$"",
  ""type"": ""string"",
  ""description"": ""The length of the appointment, denoted in ISO8601 format."",
  ""format"": ""duration"",
  ""readOnly"": true
}"), json));
            }
        }
        #endregion

        #region BaseTypeToDerivedTypesSchema

        [Fact]
        public void GetDerivedTypesReferenceSchemaReturnsDerivedTypesReferencesInSchemaIfExist()
        {
            // Arrange
            IEdmModel edmModel = EdmModelHelper.GraphBetaModel;
            OpenApiDocument openApiDocument = new();
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "directoryObject");
            OpenApiSchema schema = null;

            // Act
            schema = Common.EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, edmModel, openApiDocument);
            int derivedTypesCount = edmModel.FindAllDerivedTypes(entityType).OfType<IEdmEntityType>().Count() + 1; // + 1 the base type

            // Assert
            Assert.NotNull(schema.OneOf);
            Assert.Equal(derivedTypesCount, schema.OneOf.Count);
        }

        [Fact]
        public void GetDerivedTypesReferenceSchemaReturnsNullSchemaIfNotExist()
        {
            // Arrange
            IEdmModel edmModel = EdmModelHelper.GraphBetaModel;
            OpenApiDocument openApiDocument = new();
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "administrativeUnit");
            OpenApiSchema schema = null;

            // Act
            schema = Common.EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, edmModel, openApiDocument);

            // Assert
            Assert.Null(schema);
        }

        #endregion

        [Fact]
        public async Task NonNullableBooleanPropertyWithDefaultValueWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "BooleanValue", EdmCoreModel.Instance.GetBoolean(false), "false");

            // Act
            var schema = context.CreatePropertySchema(property, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(JsonSchemaType.Boolean, schema.Type);

            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""boolean"",
  ""default"": false
}"), JsonNode.Parse(json)));
        }

        [Fact]
        public async Task NonNullableBinaryPropertyWithBothMaxLengthAndDefaultValueWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            var binaryType = new EdmBinaryTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Binary),
                false, false, 44);
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "BinaryValue", binaryType, "T0RhdGE");

            // Act
            var schema = context.CreatePropertySchema(property, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(JsonSchemaType.String, schema.Type);

            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""maxLength"": 44,
  ""type"": ""string"",
  ""format"": ""base64url"",
  ""default"": ""T0RhdGE""
}"), JsonNode.Parse(json)));
        }

        [Fact]
        public async Task NonNullableIntegerPropertyWithDefaultValueWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "IntegerValue", EdmCoreModel.Instance.GetInt32(false), "-128");

            // Act
            var schema = context.CreatePropertySchema(property, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Equal(JsonSchemaType.Number, schema.Type);

            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""number"",
  ""format"": ""int32"",
  ""default"": -128
}"), JsonNode.Parse(json)));
        }

        [Fact]
        public async Task NonNullableDoublePropertyWithDefaultStringWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "DoubleValue", EdmCoreModel.Instance.GetDouble(false), "3.1415926535897931");

            // Act
            var schema = context.CreatePropertySchema(property, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type);

            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""oneOf"": [
    {
      ""type"": ""number"",
      ""format"": ""double"",
      ""nullable"": true
    },
    {
      ""type"": ""string"",
      ""nullable"": true
    },
    {
      ""$ref"": ""#/components/schemas/ReferenceNumeric""
    }
  ],
  ""default"": ""3.1415926535897931""
}"), JsonNode.Parse(json)));
        }


        [Fact]
        public async Task NonNullableUntypedPropertyWorks()
        {
            ODataContext context = new ODataContext(
                EdmModelHelper.BasicEdmModel,
                new OpenApiConvertSettings 
                { 
                    ShowSchemaExamples = true
                });
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entityType, "UntypedProperty", EdmCoreModel.Instance.GetUntyped());

            // Act
            var schema = context.CreatePropertySchema(property, new());

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type);

            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal("{ }", json);
        }
    }
}
