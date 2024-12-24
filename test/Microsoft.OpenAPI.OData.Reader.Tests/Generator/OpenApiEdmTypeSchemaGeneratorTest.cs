// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
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
            Assert.Throws<ArgumentNullException>("context", () => context.CreateEdmTypeSchema(edmTypeReference: null));
        }

        [Fact]
        public void CreateEdmTypeSchemaThrowArgumentNullEdmTypeReference()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act & Assert
            Assert.Throws<ArgumentNullException>("edmTypeReference", () => context.CreateEdmTypeSchema(edmTypeReference: null));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void CreateEdmTypeSchemaReturnSchemaForNullableCollectionComplexType(OpenApiSpecVersion specVersion)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            ODataContext context = new ODataContext(model);

            context.Settings.OpenApiSpecVersion = specVersion;

            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(new EdmComplexTypeReference(complex, true)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(context.Settings.OpenApiSpecVersion);

            // & Assert
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/definitions/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/components/schemas/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}".ChangeLineBreaks(), json);
            }           
        }

        [Fact]
        public void CreateEdmTypeSchemaReturnSchemaForNonNullableCollectionComplexType()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(new EdmComplexTypeReference(complex, false)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/components/schemas/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateEdmTypeSchemaReturnSchemaForNonNullableCollectionPrimitiveType()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(EdmCoreModel.Instance.GetString(false)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""type"": ""string""
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateEdmTypeSchemaReturnSchemaForNullableCollectionPrimitiveType()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(EdmCoreModel.Instance.GetInt32(true)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""maximum"": 2147483647,
    ""minimum"": -2147483648,
    ""type"": ""number"",
    ""format"": ""int32"",
    ""nullable"": true
  }
}".ChangeLineBreaks(), json);
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
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
            var schema = context.CreateEdmTypeSchema(enumTypeReference);

            // & Assert
            Assert.NotNull(schema);

            
            if (specVersion == OpenApiSpecVersion.OpenApi2_0)
            {
                Assert.NotNull(schema.Reference);
                Assert.Null(schema.AnyOf);
                Assert.Equal(ReferenceType.Schema, schema.Reference.Type);
                Assert.Equal(enumType.FullTypeName(), schema.Reference.Id);
                Assert.Equal(isNullable, schema.Nullable);
            }
            else
            {
                
                if (isNullable)
                {
                    Assert.NotNull(schema.AnyOf);
                    Assert.NotEmpty(schema.AnyOf);
                    Assert.Null(schema.Reference);
                    Assert.Equal(2, schema.AnyOf.Count);
                    var anyOfRef = schema.AnyOf.FirstOrDefault();
                    Assert.NotNull(anyOfRef.Reference);
                    Assert.Equal(ReferenceType.Schema, anyOfRef.Reference.Type);
                    Assert.Equal(enumType.FullTypeName(), anyOfRef.Reference.Id);
                    var anyOfNull = schema.AnyOf.Skip(1).FirstOrDefault();
                    Assert.NotNull(anyOfNull.Type);
                    Assert.Equal("object", anyOfNull.Type);
                    Assert.True(anyOfNull.Nullable);
                }
                else
                {
                    Assert.Null(schema.AnyOf);
                    Assert.NotNull(schema.Reference);
                    Assert.Equal(ReferenceType.Schema, schema.Reference.Type);
                    Assert.Equal(enumType.FullTypeName(), schema.Reference.Id);
                }             
            }
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
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
            var schema = context.CreateEdmTypeSchema(complexTypeReference);

            // & Assert
            Assert.NotNull(schema);

            if (specVersion == OpenApiSpecVersion.OpenApi2_0 || isNullable == false)
            {
                Assert.Null(schema.AnyOf);
                Assert.NotNull(schema.Reference);
                Assert.Equal(ReferenceType.Schema, schema.Reference.Type);
                Assert.Equal(complex.FullTypeName(), schema.Reference.Id);
            }
            else
            {
                Assert.Null(schema.Reference);
                Assert.NotNull(schema.AnyOf);
                Assert.NotEmpty(schema.AnyOf);
                Assert.Equal(2, schema.AnyOf.Count);
                var anyOf = schema.AnyOf.FirstOrDefault();
                Assert.NotNull(anyOf.Reference);
                Assert.Equal(ReferenceType.Schema, anyOf.Reference.Type);
                Assert.Equal(complex.FullTypeName(), anyOf.Reference.Id);
            }
        }

        [Theory]
        [InlineData(true, OpenApiSpecVersion.OpenApi3_0)]
        [InlineData(true, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi2_0)]
        [InlineData(false, OpenApiSpecVersion.OpenApi3_0)]
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
            var schema = context.CreateEdmTypeSchema(entityTypeReference);

            // & Assert
            Assert.NotNull(schema);

            if (specVersion == OpenApiSpecVersion.OpenApi2_0 || isNullable == false)
            {
                Assert.Null(schema.AnyOf);
                Assert.NotNull(schema.Reference);
                Assert.Equal(ReferenceType.Schema, schema.Reference.Type);
                Assert.Equal(entity.FullTypeName(), schema.Reference.Id);
            }
            else
            {
                Assert.Null(schema.Reference);
                Assert.NotNull(schema.AnyOf);
                Assert.NotEmpty(schema.AnyOf);
                var anyOfRef = schema.AnyOf.FirstOrDefault();
                Assert.NotNull(anyOfRef.Reference);
                Assert.Equal(ReferenceType.Schema, anyOfRef.Reference.Type);
                Assert.Equal(entity.FullTypeName(), anyOfRef.Reference.Id);
                var anyOfNull = schema.AnyOf.Skip(1).FirstOrDefault();
                Assert.NotNull(anyOfNull.Type);
                Assert.Equal("object", anyOfNull.Type);
                Assert.True(anyOfNull.Nullable);
            }
        }

        #region Primitive type schema

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForString(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetString(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            if (isNullable)
            {
                Assert.Equal(@"{
  ""type"": ""string"",
  ""nullable"": true
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""type"": ""string""
}".ChangeLineBreaks(), json);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForInt32(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetInt32(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            if (isNullable)
            {
                Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""number"",
  ""format"": ""int32"",
  ""nullable"": true
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""number"",
  ""format"": ""int32""
}".ChangeLineBreaks(), json);
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
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard

            // & Assert
            if (IEEE754Compatible)
            {
                Assert.Null(schema.Type);
                Assert.NotNull(schema.OneOf);
                Assert.Equal(2, schema.OneOf.Count);
                var numberSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("number", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(numberSchema);
                Assert.True(numberSchema.Nullable);
                var stringSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("string", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(stringSchema);
                Assert.True(stringSchema.Nullable);
                Assert.False(schema.Nullable);
            }
            else
            {
                Assert.Equal("number", schema.Type);
                Assert.Null(schema.OneOf);
                Assert.Equal(isNullable, schema.Nullable);
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
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard

            // & Assert
            if (IEEE754Compatible)
            {
                Assert.Null(schema.Type);
                Assert.NotNull(schema.OneOf);
                Assert.Equal(2, schema.OneOf.Count);
                var numberSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("number", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(numberSchema);
                Assert.True(numberSchema.Nullable);
                var stringSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("string", StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(stringSchema);
                Assert.True(stringSchema.Nullable);
                Assert.False(schema.Nullable);
            }
            else
            {
                Assert.Equal("number", schema.Type);
                Assert.Null(schema.AnyOf);
                Assert.Equal(isNullable, schema.Nullable);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForGuid(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            IEdmTypeReference edmTypeReference = EdmCoreModel.Instance.GetGuid(isNullable);

            // Act
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            if (isNullable)
            {
                Assert.Equal(@"{
  ""pattern"": ""^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"",
  ""type"": ""string"",
  ""format"": ""uuid"",
  ""nullable"": true
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""pattern"": ""^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"",
  ""type"": ""string"",
  ""format"": ""uuid""
}".ChangeLineBreaks(), json);
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
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard

            // & Assert
            Assert.Null(schema.Type);

            var numberSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("number", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(numberSchema);
            Assert.True(numberSchema.Nullable);
            Assert.Equal("double", numberSchema.Format, StringComparer.OrdinalIgnoreCase);

            var stringSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("string", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(stringSchema);
            Assert.True(stringSchema.Nullable);

            Assert.False(schema.Nullable);

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
            var schema = context.CreateEdmTypeSchema(edmTypeReference);
            Assert.NotNull(schema); // guard

            // & Assert
            Assert.Null(schema.Type);

            var numberSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("number", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(numberSchema);
            Assert.True(numberSchema.Nullable);
            Assert.Equal("float", numberSchema.Format, StringComparer.OrdinalIgnoreCase);

            var stringSchema = schema.OneOf.FirstOrDefault(x => x.Type.Equals("string", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(stringSchema);
            Assert.True(stringSchema.Nullable);
            
            Assert.False(schema.Nullable);

            Assert.Null(schema.AnyOf);

            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
        }
        #endregion
    }
}
