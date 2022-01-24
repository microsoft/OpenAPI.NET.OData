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

        [Fact]
        public void CreatesCollectionResponseSchema()
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

            var flightCollectionResponse = schemas["Microsoft.OData.Service.Sample.TrippinInMemory.Models.FlightCollectionResponse"];
            var stringCollectionResponse = schemas["StringCollectionResponse"];

            Assert.Equal("array", flightCollectionResponse.Properties["value"].Type);
            Assert.Equal("Microsoft.OData.Service.Sample.TrippinInMemory.Models.Flight", flightCollectionResponse.Properties["value"].Items.Reference.Id);
            Assert.Equal("array", stringCollectionResponse.Properties["value"].Type);
            Assert.Equal("string", stringCollectionResponse.Properties["value"].Items.Type);
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
            Assert.Equal(1, declaredSchema.Properties.Count);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Price", property.Key);
            Assert.Equal("decimal", property.Value.Format);
            Assert.NotNull(property.Value.AnyOf);
            Assert.Equal(new string[] { "number", "string" }, property.Value.AnyOf.Select(e => e.Type));

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
          ""multipleOf"": 1,
          ""anyOf"": [
            {
              ""type"": ""number""
            },
            {
              ""type"": ""string""
            }
          ],
          ""format"": ""decimal""
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
      ""type"": ""integer"",
      ""format"": ""int32""
    },
    ""Creatures"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/components/schemas/NS.Creature""
      }
    }
  },
  ""description"": ""Entity type 'Zoo' description."",
  ""example"": {
    ""Id"": ""integer (identifier)"",
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
            Assert.Equal(1, declaredSchema.Properties.Count);
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
    ""Id"": ""integer (identifier)"",
    ""Age"": ""integer"",
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
            Assert.Equal(1, declaredSchema.Properties.Count);
            var property = Assert.Single(declaredSchema.Properties);
            Assert.Equal("Extra", property.Key);
            Assert.Equal("integer", property.Value.Type);
            Assert.Null(property.Value.OneOf);

            Assert.Equal("Customer", declaredSchema.Title);
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
            IEdmProperty property = new EdmStructuralProperty(entitType, "ColorEnumValue", new EdmEnumTypeReference(enumType, false), "yellow");

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
  ""anyOf"": [
    {
      ""$ref"": ""#/components/schemas/DefaultNs.Color""
    }
  ],
  ""default"": ""yellow""
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
    }
  ],
  ""default"": ""yellow"",
  ""nullable"": true
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
            int derivedTypesCount = edmModel.FindDirectlyDerivedTypes(entityType).OfType<IEdmEntityType>().Count() + 1; // + 1 the base type

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
            Assert.Equal("integer", schema.Type);

            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""integer"",
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
  ""anyOf"": [
    {
      ""type"": ""number""
    },
    {
      ""type"": ""string""
    },
    {
      ""enum"": [
        ""-INF"",
        ""INF"",
        ""NaN""
      ]
    }
  ],
  ""format"": ""double"",
  ""default"": ""3.1415926535897931""
}".ChangeLineBreaks(), json);
        }
    }
}
