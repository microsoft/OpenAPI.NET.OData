// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiSchemaGeneratorTest
    {
        private ITestOutputHelper _output;
        public OpenApiSchemaGeneratorTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CreateSchemasThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateSchemas());
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
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = enablePagination,
                EnableCount = enableCount
            };
            ODataContext context = new(model, settings);

            // Act & Assert
            var schemas = context.CreateSchemas();

            var stringCollectionResponse = schemas["StringCollectionResponse"];
            var flightCollectionResponse = schemas["Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse"];

            if (enablePagination || enableCount)
            {
                Assert.Collection(stringCollectionResponse.AllOf,
                item =>
                {
                    Assert.Equal(referenceId, item.Reference.Id);
                },
                item =>
                {
                    Assert.Equal("array", item.Properties["value"].Type);
                });

                Assert.Equal("array", flightCollectionResponse.AllOf?.FirstOrDefault(x => x.Properties.Any())?.Properties["value"].Type);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Flight",
                    flightCollectionResponse.AllOf?.FirstOrDefault(x => x.Properties.Any())?.Properties["value"].Items.Reference.Id);
            }
            else
            {
                Assert.Equal("array", stringCollectionResponse.Properties["value"].Type);
                Assert.Equal("array", flightCollectionResponse.Properties["value"].Type);
                Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Flight", flightCollectionResponse.Properties["value"].Items.Reference.Id);
            }
        }

        [Fact]
        public void CreatesRefRequestBodySchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = true,
            };
            ODataContext context = new(model, settings);

            // Act & Assert
            var schemas = context.CreateSchemas();

            schemas.TryGetValue(Constants.ReferenceCreateSchemaName, out OpenApiSchema refRequestBody);

            Assert.NotNull(refRequestBody);
            Assert.Equal("object", refRequestBody.Type);
            Assert.Equal(Constants.OdataId, refRequestBody.Properties.First().Key);
            Assert.Equal("string", refRequestBody.Properties.First().Value.Type);
            Assert.Equal("object", refRequestBody.AdditionalProperties.Type);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatesRefOdataAnnotationResponseSchemas(bool enableOdataAnnotationRef)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            OpenApiConvertSettings settings = new()
            {
                EnableOperationId = true,
                EnablePagination = true,
                EnableCount = true,
                EnableODataAnnotationReferencesForResponses = enableOdataAnnotationRef
            };
            ODataContext context = new(model, settings);

            // Act
            var schemas = context.CreateSchemas();

            // Assert
            Assert.NotNull(schemas);
            Assert.NotEmpty(schemas);
            schemas.TryGetValue(Constants.BaseCollectionPaginationCountResponse, out OpenApiSchema refPaginationCount);
            schemas.TryGetValue(Constants.BaseDeltaFunctionResponse, out OpenApiSchema refDeltaFunc);
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
            Assert.Throws<ArgumentNullException>("context", () => context.CreateStructuredTypeSchema(structuredType: null));
        }

        [Fact]
        public void CreateStructuredTypeSchemaThrowArgumentNullEnumType()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("structuredType", () => context.CreateStructuredTypeSchema(structuredType: null));
        }

        [Fact]
        public void CreateStructuredTypeSchemaForEntityTypeWithDiscriminatorValueEnabledReturnsCorrectSchema()
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
            var schema = context.CreateStructuredTypeSchema(entity);
            var derivedSchema = context.CreateStructuredTypeSchema(derivedEntity);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.True(derivedSchema.AllOf.FirstOrDefault(x => derivedType.Equals(x.Title))?.Properties.ContainsKey("@odata.type"));
            Assert.NotNull(json);
            Assert.Equal(@"{
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
}".ChangeLineBreaks(), json);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateStructuredTypeSchemaForComplexTypeWithDiscriminatorValueEnabledReturnsCorrectSchema(bool enableTypeDisambiguationForOdataTypePropertyDefaultValue)
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
            var schema = context.CreateStructuredTypeSchema(complex);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

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
}".ChangeLineBreaks()
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
}".ChangeLineBreaks();

            Assert.Equal(expected, json);
        }

        [Fact]
        public void CreateStructuredTypePropertiesSchemaWithCustomAttributeReturnsCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model);
            context.Settings.CustomXMLAttributesMapping.Add("IsHidden", "x-ms-isHidden");
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(t => t.Name == "userSettings");
            Assert.NotNull(entity); // Guard

            // Act
            OpenApiSchema schema = context.CreateStructuredTypeSchema(entity);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.Equal(@"{
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
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateComplexTypeWithoutBaseSchemaReturnCorrectSchema()
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
            var schema = context.CreateStructuredTypeSchema(complex);

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("object", schema.Type);
            Assert.Null(schema.AllOf);

            Assert.NotNull(schema.Properties);
            Assert.Equal(2, schema.Properties.Count);
            Assert.Equal(new string[] { "Street", "City" }, schema.Properties.Select(e => e.Key));
            Assert.Equal("Complex type 'Address' description.", schema.Description);
            Assert.Equal("Address", schema.Title);

            // Act
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.Equal(@"{
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
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateComplexTypeWithBaseSchemaReturnCorrectSchema()
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
            var schema = context.CreateStructuredTypeSchema(complex);

            // Assert
            Assert.NotNull(schema);
            Assert.True(String.IsNullOrEmpty(schema.Type));

            Assert.NotNull(schema.AllOf);
            Assert.Null(schema.AnyOf);
            Assert.Null(schema.OneOf);
            Assert.Null(schema.Properties);

            Assert.Equal(2, schema.AllOf.Count);
            var baseSchema = schema.AllOf.First();
            Assert.NotNull(baseSchema.Reference);
            Assert.Equal(ReferenceType.Schema, baseSchema.Reference.Type);
            Assert.Equal("NS.LandPlant", baseSchema.Reference.Id);

            var declaredSchema = schema.AllOf.Last();
            Assert.Equal("object", declaredSchema.Type);
            Assert.Null(declaredSchema.AllOf);
            Assert.Null(declaredSchema.AnyOf);
            Assert.Null(declaredSchema.OneOf);

            Assert.NotNull(declaredSchema.Properties);
            Assert.Single(declaredSchema.Properties);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Price", property.Key);
            Assert.Equal("decimal", property.Value.OneOf.FirstOrDefault(x => !string.IsNullOrEmpty(x.Format))?.Format);
            Assert.NotNull(property.Value.OneOf);
            Assert.Equal(new string[] { "number", "string" }, property.Value.OneOf.Select(e => e.Type));

            Assert.Equal("Complex type 'Tree' description.", declaredSchema.Description);
            Assert.Equal("Tree", declaredSchema.Title);

            // Act
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.Equal(@"{
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
    ""Color"": {
      ""@odata.type"": ""NS.Color""
    },
    ""Continent"": {
      ""@odata.type"": ""NS.Continent""
    },
    ""Name"": ""string"",
    ""Price"": ""decimal""
  }
}"
.ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateEntityTypeWithoutBaseSchemaReturnCorrectSchema()
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
            var schema = context.CreateStructuredTypeSchema(entity);

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("object", schema.Type);
            Assert.Null(schema.AllOf);

            Assert.NotNull(schema.Properties);
            Assert.Equal(2, schema.Properties.Count);
            Assert.Equal(new string[] { "Id", "Creatures" }, schema.Properties.Select(e => e.Key));
            Assert.Equal("Entity type 'Zoo' description.", schema.Description);
            Assert.Equal("Zoo", schema.Title);

            // Act
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.Equal(@"{
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
    ""Id"": ""number (identifier)"",
    ""Creatures"": [
      {
        ""@odata.type"": ""NS.Creature""
      }
    ]
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateEntityTypeWithBaseSchemaReturnCorrectSchema()
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
            var schema = context.CreateStructuredTypeSchema(entity);

            // Assert
            Assert.NotNull(schema);
            Assert.True(String.IsNullOrEmpty(schema.Type));

            Assert.NotNull(schema.AllOf);
            Assert.Null(schema.AnyOf);
            Assert.Null(schema.OneOf);
            Assert.Null(schema.Properties);

            Assert.Equal(2, schema.AllOf.Count);
            var baseSchema = schema.AllOf.First();
            Assert.NotNull(baseSchema.Reference);
            Assert.Equal(ReferenceType.Schema, baseSchema.Reference.Type);
            Assert.Equal("NS.Animal", baseSchema.Reference.Id);

            var declaredSchema = schema.AllOf.Last();
            Assert.Equal("object", declaredSchema.Type);
            Assert.Null(declaredSchema.AllOf);
            Assert.Null(declaredSchema.AnyOf);
            Assert.Null(declaredSchema.OneOf);

            Assert.NotNull(declaredSchema.Properties);
            Assert.Single(declaredSchema.Properties);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Name", property.Key);
            Assert.Equal("string", property.Value.Type);
            Assert.Null(property.Value.OneOf);

            Assert.Equal("Entity type 'Human' description.", declaredSchema.Description);
            Assert.Equal("Human", declaredSchema.Title);

            // Act
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            _output.WriteLine(json);
            // Assert
            Assert.NotNull(json);
            Assert.Equal(@"{
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
    ""Id"": ""number (identifier)"",
    ""Age"": ""number"",
    ""Name"": ""string""
  }
}"
.ChangeLineBreaks(), json);
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
            var schema = context.CreateStructuredTypeSchema(entity);

            // Assert
            Assert.NotNull(schema);
            Assert.True(String.IsNullOrEmpty(schema.Type));

            Assert.NotNull(schema.AllOf);
            Assert.Null(schema.AnyOf);
            Assert.Null(schema.OneOf);
            Assert.Null(schema.Properties);

            Assert.Equal(2, schema.AllOf.Count);
            var baseSchema = schema.AllOf.First();
            Assert.NotNull(baseSchema.Reference);
            Assert.Equal(ReferenceType.Schema, baseSchema.Reference.Type);
            Assert.Equal("SubNS.CustomerBase", baseSchema.Reference.Id);

            var declaredSchema = schema.AllOf.Last();
            Assert.Equal("object", declaredSchema.Type);
            Assert.Null(declaredSchema.AllOf);
            Assert.Null(declaredSchema.AnyOf);
            Assert.Null(declaredSchema.OneOf);

            Assert.NotNull(declaredSchema.Properties);
            Assert.Single(declaredSchema.Properties);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Extra", property.Key);
            Assert.Equal("number", property.Value.Type);
            Assert.Null(property.Value.OneOf);

            Assert.Equal("Customer", declaredSchema.Title);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateStructuredTypeSchemaForEntityTypeWithDefaultValueForOdataTypePropertyEnabledOrDisabledReturnsCorrectSchema(bool enableTypeDisambiguationForOdataTypePropertyDefaultValue)
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
            var schema = context.CreateStructuredTypeSchema(entityType);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

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
        public void CreateEnumTypeSchemaReturnCorrectSchema()
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
            Assert.Equal("string", schema.Type);
            Assert.Equal("Enum type 'Color' description.", schema.Description);
            Assert.Equal("Color", schema.Title);

            Assert.NotNull(schema.Enum);
            Assert.Equal(2, schema.Enum.Count);
            Assert.Equal(new string[] { "Blue", "White" }, schema.Enum.Select(e => ((OpenApiString)e).Value));

            // Act
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.NotNull(json);
            Assert.Equal(@"{
  ""title"": ""Color"",
  ""enum"": [
    ""Blue"",
    ""White""
  ],
  ""type"": ""string"",
  ""description"": ""Enum type 'Color' description.""
}".ChangeLineBreaks(), json);
        }
        #endregion

        #region EdmPropertySchema
        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void CreatePropertySchemaForNonNullableEnumPropertyReturnSchema(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(e => e.Name == "Color");
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmProperty property = new EdmStructuralProperty(entitType, "ColorEnumValue", new EdmEnumTypeReference(enumType, false));

            // Act
            var schema = context.CreatePropertySchema(property);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(specVersion);

            // Assert

            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(@"{
  ""$ref"": ""#/definitions/DefaultNs.Color""
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/DefaultNs.Color""
}".ChangeLineBreaks(), json);
            }
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        public void CreatePropertySchemaForNullableEnumPropertyReturnSchema(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(e => e.Name == "Color");
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmProperty property = new EdmStructuralProperty(entitType, "ColorEnumValue", new EdmEnumTypeReference(enumType, true), "yellow");

            // Act
            var schema = context.CreatePropertySchema(property);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(specVersion);
            _output.WriteLine(json);

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(@"{
  ""$ref"": ""#/definitions/DefaultNs.Color""
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""anyOf"": [
    {
      ""$ref"": ""#/components/schemas/DefaultNs.Color""
    },
    {
      ""type"": ""object"",
      ""nullable"": true
    }
  ],
  ""default"": ""yellow""
}".ChangeLineBreaks(), json);
            }
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        public void CreatePropertySchemaWithComputedAnnotationReturnsCorrectSchema(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "bookingAppointment");
            IEdmProperty property = entityType.Properties().FirstOrDefault(x => x.Name == "duration");

            // Act
            var schema = context.CreatePropertySchema(property);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(specVersion);

            // Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(@"{
  ""format"": ""duration"",
  ""description"": ""The length of the appointment, denoted in ISO8601 format."",
  ""pattern"": ""^-?P([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+([.][0-9]+)?S)?)?$"",
  ""type"": ""string"",
  ""readOnly"": true
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""pattern"": ""^-?P([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+([.][0-9]+)?S)?)?$"",
  ""type"": ""string"",
  ""description"": ""The length of the appointment, denoted in ISO8601 format."",
  ""format"": ""duration"",
  ""readOnly"": true
}".ChangeLineBreaks(), json);
            }
        }
        #endregion

        #region BaseTypeToDerivedTypesSchema

        [Fact]
        public void GetDerivedTypesReferenceSchemaReturnsDerivedTypesReferencesInSchemaIfExist()
        {
            // Arrange
            IEdmModel edmModel = EdmModelHelper.GraphBetaModel;
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "directoryObject");
            OpenApiSchema schema = null;

            // Act
            schema = Common.EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, edmModel);
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
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "administrativeUnit");
            OpenApiSchema schema = null;

            // Act
            schema = Common.EdmModelHelper.GetDerivedTypesReferenceSchema(entityType, edmModel);

            // Assert
            Assert.Null(schema);
        }

        #endregion

        [Fact]
        public void NonNullableBooleanPropertyWithDefaultValueWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "BooleanValue", EdmCoreModel.Instance.GetBoolean(false), "false");

            // Act
            var schema = context.CreatePropertySchema(property);

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("boolean", schema.Type);

            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""type"": ""boolean"",
  ""default"": false
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void NonNullableBinaryPropertyWithBothMaxLengthAndDefaultValueWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            var binaryType = new EdmBinaryTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Binary),
                false, false, 44);
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "BinaryValue", binaryType, "T0RhdGE");

            // Act
            var schema = context.CreatePropertySchema(property);

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("string", schema.Type);

            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""maxLength"": 44,
  ""type"": ""string"",
  ""format"": ""base64url"",
  ""default"": ""T0RhdGE""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void NonNullableIntegerPropertyWithDefaultValueWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "IntegerValue", EdmCoreModel.Instance.GetInt32(false), "-128");

            // Act
            var schema = context.CreatePropertySchema(property);

            // Assert
            Assert.NotNull(schema);
            Assert.Equal("number", schema.Type);

            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""number"",
  ""format"": ""int32"",
  ""default"": -128
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void NonNullableDoublePropertyWithDefaultStringWorks()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmModelHelper.BasicEdmModel);
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "DoubleValue", EdmCoreModel.Instance.GetDouble(false), "3.1415926535897931");

            // Act
            var schema = context.CreatePropertySchema(property);

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type);

            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal(@"{
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
}".ChangeLineBreaks(), json);
        }


        [Fact]
        public void NonNullableUntypedPropertyWorks()
        {
            ODataContext context = new ODataContext(
                EdmModelHelper.BasicEdmModel,
                new OpenApiConvertSettings 
                { 
                    ShowSchemaExamples = true
                });
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty property = new EdmStructuralProperty(
                entitType, "UntypedProperty", EdmCoreModel.Instance.GetUntyped());

            // Act
            var schema = context.CreatePropertySchema(property);

            // Assert
            Assert.NotNull(schema);
            Assert.Null(schema.Type);

            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            Assert.Equal("{ }", json);
        }
    }
}
