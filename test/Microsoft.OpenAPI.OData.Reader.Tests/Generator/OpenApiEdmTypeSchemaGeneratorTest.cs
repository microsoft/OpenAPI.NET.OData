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
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiEdmTypeSchemaGeneratorTest
    {
        private ITestOutputHelper _output;
        public OpenApiEdmTypeSchemaGeneratorTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CreateEdmTypeSchemaThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEdmTypeSchema(edmTypeReference: null, new()));
        }

        [Fact]
        public void CreateEdmTypeSchemaThrowArgumentNullEdmTypeReference()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("edmTypeReference", () => context.CreateEdmTypeSchema(edmTypeReference: null, new()));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public async Task CreateEdmTypeSchemaReturnSchemaForNullableCollectionComplexType(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(new EdmComplexTypeReference(complex, true)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType, new());
            Assert.NotNull(schema);
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(context.Settings.OpenApiSpecVersion));

            // & Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/definitions/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}"), json));
            }
            else
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/components/schemas/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}"), json));
            }           
        }

        [Fact]
        public async Task CreateEdmTypeSchemaReturnSchemaForNonNullableCollectionComplexType()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(new EdmComplexTypeReference(complex, false)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType, new());
            Assert.NotNull(schema);
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // & Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/components/schemas/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}"), json));
        }

        [Fact]
        public async Task CreateEdmTypeSchemaReturnSchemaForNonNullableCollectionPrimitiveType()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(EdmCoreModel.Instance.GetString(false)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType, new());
            Assert.NotNull(schema);
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // & Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""array"",
  ""items"": {
    ""type"": ""string""
  }
}"), json));
        }

        [Fact]
        public async Task CreateEdmTypeSchemaReturnSchemaForNullableCollectionPrimitiveType()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(EdmCoreModel.Instance.GetInt32(true)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType, new());
            Assert.NotNull(schema);
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // & Assert
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""array"",
  ""items"": {
    ""maximum"": 2147483647,
    ""minimum"": -2147483648,
    ""type"": ""number"",
    ""format"": ""int32"",
    ""nullable"": true
  }
}"), json));
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_1)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_1)]
        public void CreateEdmTypeSchemaReturnSchemaForEnumType(bool isNullable, OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(c => c.Name == "PersonGender");
            Assert.NotNull(enumType); // guard
            IEdmEnumTypeReference enumTypeReference = new EdmEnumTypeReference(enumType, isNullable);
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            // Act
            var schema = context.CreateEdmTypeSchema(enumTypeReference, new());

            // & Assert
            Assert.NotNull(schema);
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                var schemaReference = Assert.IsType<OpenApiSchemaReference>(schema);
                Assert.Null(schema.AnyOf);
                Assert.Equal(ReferenceType.Schema, schemaReference.Reference.Type);
                Assert.Equal(enumType.FullTypeName(), schemaReference.Reference.Id);
            }
            else
            {
                if (isNullable)
                {
                    Assert.NotNull(schema.AnyOf);
                    Assert.NotEmpty(schema.AnyOf);
                    Assert.IsNotType<OpenApiSchemaReference>(schema);
                    Assert.Equal(2, schema.AnyOf.Count);
                    var anyOfRef = Assert.IsType<OpenApiSchemaReference>(schema.AnyOf.FirstOrDefault());
                    Assert.NotNull(anyOfRef.Reference);
                    Assert.Equal(ReferenceType.Schema, anyOfRef.Reference.Type);
                    Assert.Equal(enumType.FullTypeName(), anyOfRef.Reference.Id);
                    var anyOfNull = schema.AnyOf.Skip(1).FirstOrDefault();
                    Assert.NotNull(anyOfNull.Type);
                    Assert.Equal(JsonSchemaType.Null, anyOfNull.Type);
                }
                else
                {
                    Assert.Null(schema.AnyOf);
                    var schemaReference = Assert.IsType<OpenApiSchemaReference>(schema);
                    Assert.Equal(ReferenceType.Schema, schemaReference.Reference.Type);
                    Assert.Equal(enumType.FullTypeName(), schemaReference.Reference.Id);
                }
            }
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_1)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_1)]
        public void CreateEdmTypeSchemaReturnSchemaForComplexType(bool isNullable, OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            Assert.NotNull(complex); // guard
            IEdmComplexTypeReference complexTypeReference = new EdmComplexTypeReference(complex, isNullable);
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            // Act
            var schema = context.CreateEdmTypeSchema(complexTypeReference, new());

            // & Assert
            Assert.NotNull(schema);

            if (specVersion == OpenApiSpecVersion.OpenApi2_0 || isNullable == false)
            {
                Assert.Null(schema.AnyOf);
                var schemaReference = Assert.IsType<OpenApiSchemaReference>(schema);
                Assert.Equal(ReferenceType.Schema, schemaReference.Reference.Type);
                Assert.Equal(complex.FullTypeName(), schemaReference.Reference.Id);
            }
            else
            {
                Assert.IsNotType<OpenApiSchemaReference>(schema);
                Assert.NotNull(schema.AnyOf);
                Assert.NotEmpty(schema.AnyOf);
                Assert.Equal(2, schema.AnyOf.Count);
                var anyOf = Assert.IsType<OpenApiSchemaReference>(schema.AnyOf.FirstOrDefault());
                Assert.NotNull(anyOf.Reference);
                Assert.Equal(ReferenceType.Schema, anyOf.Reference.Type);
                Assert.Equal(complex.FullTypeName(), anyOf.Reference.Id);
            }
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_1)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_1)]
        public void CreateEdmTypeSchemaReturnSchemaForEntityType(bool isNullable, OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Manager");
            Assert.NotNull(entity); // guard
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entity, isNullable);
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            // Act
            var schema = context.CreateEdmTypeSchema(entityTypeReference, new());

            // & Assert
            Assert.NotNull(schema);

            if (specVersion == OpenApiSpecVersion.OpenApi2_0 || !isNullable)
            {
                Assert.Null(schema.AnyOf);
                var schemaReference = Assert.IsType<OpenApiSchemaReference>(schema);
                Assert.NotNull(schemaReference.Reference);
                Assert.Equal(ReferenceType.Schema, schemaReference.Reference.Type);
                Assert.Equal(entity.FullTypeName(), schemaReference.Reference.Id);
            }
            else
            {
                Assert.IsNotType<OpenApiSchemaReference>(schema);
                Assert.NotNull(schema.AnyOf);
                Assert.NotEmpty(schema.AnyOf);
                var anyOfRef = Assert.IsType<OpenApiSchemaReference>(schema.AnyOf.FirstOrDefault());
                Assert.NotNull(anyOfRef.Reference);
                Assert.Equal(ReferenceType.Schema, anyOfRef.Reference.Type);
                Assert.Equal(entity.FullTypeName(), anyOfRef.Reference.Id);
                var anyOfNull = schema.AnyOf.Skip(1).FirstOrDefault();
                Assert.NotNull(anyOfNull.Type);
                Assert.Equal(JsonSchemaType.Null, anyOfNull.Type);
            }
        }

        #region Primitive type schema

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateEdmTypeSchemaReturnSchemaForString(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetString(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // & Assert
            if (isNullable)
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""string"",
  ""nullable"": true
}"), json));
            }
            else
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""type"": ""string""
}"), json));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateEdmTypeSchemaReturnSchemaForInt32(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetInt32(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // & Assert
            if (isNullable)
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""number"",
  ""format"": ""int32"",
  ""nullable"": true
}"), json));
            }
            else
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""number"",
  ""format"": ""int32""
}"), json));
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CreateEdmTypeSchemaReturnSchemaForDecimal(bool isNullable, bool IEEE754Compatible)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                IEEE754Compatible = IEEE754Compatible
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetDecimal(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard

            // & Assert
            if (IEEE754Compatible)
            {
                Assert.Null(schema.Type);
                Assert.NotNull(schema.OneOf);
                Assert.Equal(2, schema.OneOf.Count);
                Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.Number | JsonSchemaType.Null));
                Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.String | JsonSchemaType.Null));
                Assert.NotEqual(JsonSchemaType.Null, schema.Type & JsonSchemaType.Null);
            }
            else
            {
                Assert.Equal(JsonSchemaType.Number, schema.Type & JsonSchemaType.Number);
                Assert.Null(schema.OneOf);
                Assert.Equal(isNullable, (schema.Type & JsonSchemaType.Null) is JsonSchemaType.Null);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CreateEdmTypeSchemaReturnSchemaForInt64(bool isNullable, bool IEEE754Compatible)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                IEEE754Compatible = IEEE754Compatible
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetInt64(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard

            // & Assert
            if (IEEE754Compatible)
            {
                Assert.Null(schema.Type);
                Assert.NotNull(schema.OneOf);
                Assert.Equal(2, schema.OneOf.Count);
                Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.Number | JsonSchemaType.Null));
                Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.String | JsonSchemaType.Null));
                Assert.NotEqual(JsonSchemaType.Null, schema.Type & JsonSchemaType.Null);
            }
            else
            {
                Assert.Equal(JsonSchemaType.Number, schema.Type & JsonSchemaType.Number);
                Assert.Null(schema.AnyOf);
                Assert.Equal(isNullable, (schema.Type & JsonSchemaType.Null) is JsonSchemaType.Null);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateEdmTypeSchemaReturnSchemaForGuid(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetGuid(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard
            var json = JsonNode.Parse(await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));

            // & Assert
            if (isNullable)
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""pattern"": ""^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"",
  ""type"": ""string"",
  ""format"": ""uuid"",
  ""nullable"": true
}"), json));
            }
            else
            {
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(@"{
  ""pattern"": ""^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"",
  ""type"": ""string"",
  ""format"": ""uuid""
}"), json));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForDouble(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetDouble(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard

            // & Assert
            Assert.Null(schema.Type);

            Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.Number | JsonSchemaType.Null) && x.Format.Equals("double", StringComparison.OrdinalIgnoreCase));

            Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.String | JsonSchemaType.Null));

            Assert.NotEqual(JsonSchemaType.Null, schema.Type & JsonSchemaType.Null);

            Assert.Null(schema.AnyOf);

            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForSingle(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetSingle(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference, new());
            Assert.NotNull(schema); // guard

            // & Assert
            Assert.Null(schema.Type);

            Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.Number | JsonSchemaType.Null) && x.Format.Equals("float", StringComparison.OrdinalIgnoreCase));

            Assert.Single(schema.OneOf, x => x.Type.Equals(JsonSchemaType.String | JsonSchemaType.Null));

            Assert.NotEqual(JsonSchemaType.Null, schema.Type & JsonSchemaType.Null);

            Assert.Null(schema.AnyOf);

            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
        }
        #endregion
    }
}
