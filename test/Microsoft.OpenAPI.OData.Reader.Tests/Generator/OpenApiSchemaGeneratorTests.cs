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
            ODataContext context = new ODataContext(model);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

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
  ""description"": ""Complex type 'Address' description.""
}"
.Replace(), json);
        }

        [Fact]
        public void CreateComplexTypeWithBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model);
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
            Assert.NotNull(property.Value.OneOf);
            Assert.Equal(new string[] { "number", "string" }, property.Value.OneOf.Select(e => e.Type));

            Assert.Equal("Complex type 'Tree' description.", declaredSchema.Description);
            Assert.Equal("Tree", declaredSchema.Title);

            // Act
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

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
          ""oneOf"": [
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
  ]
}"
.Replace(), json);
        }

        [Fact]
        public void CreateEntityTypeWithoutBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

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
  ""description"": ""Entity type 'Zoo' description.""
}"
.Replace(), json);
        }

        [Fact]
        public void CreateEntityTypeWithBaseSchemaReturnCorrectSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.MultipleInheritanceEdmModel;
            ODataContext context = new ODataContext(model);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);
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
  ]
}"
.Replace(), json);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

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
}".Replace(), json);
        }
        #endregion

        #region EdmPropertySchema
        [Fact]
        public void CreatePropertySchemaForNonNullableEnumPropertyReturnSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(e => e.Name == "Color");
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmProperty property = new EdmStructuralProperty(entitType, "ColorEnumValue", new EdmEnumTypeReference(enumType, false), "yellow");

            // Act
            var schema = context.CreatePropertySchema(property);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            // Assert
            Assert.Equal(@"{
  ""anyOf"": [
    {
      ""$ref"": ""#/components/schemas/DefaultNs.Color""
    }
  ],
  ""default"": ""yellow""
}".Replace(), json);
        }

        [Fact]
        public void CreatePropertySchemaForNullableEnumPropertyReturnSchema()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            ODataContext context = new ODataContext(model);
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(e => e.Name == "Color");
            EdmEntityType entitType = new EdmEntityType("NS", "Entity");
            IEdmProperty property = new EdmStructuralProperty(entitType, "ColorEnumValue", new EdmEnumTypeReference(enumType, true), "yellow");

            // Act
            var schema = context.CreatePropertySchema(property);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);
            _output.WriteLine(json);
            // Assert
            Assert.Equal(@"{
  ""anyOf"": [
    {
      ""$ref"": ""#/components/schemas/DefaultNs.Color""
    }
  ],
  ""default"": ""yellow"",
  ""nullable"": true
}".Replace(), json);
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

            string json = schema.SerializeAsJson();
            Assert.Equal(@"{
  ""type"": ""boolean"",
  ""default"": false
}".Replace(), json);
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

            string json = schema.SerializeAsJson();
            Assert.Equal(@"{
  ""maxLength"": 44,
  ""type"": ""string"",
  ""format"": ""base64url"",
  ""default"": ""T0RhdGE""
}".Replace(), json);
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

            string json = schema.SerializeAsJson();
            Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""integer"",
  ""format"": ""int32"",
  ""default"": -128
}".Replace(), json);
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

            string json = schema.SerializeAsJson();

            Assert.Equal(@"{
  ""oneOf"": [
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
}".Replace(), json);
        }
    }
}
