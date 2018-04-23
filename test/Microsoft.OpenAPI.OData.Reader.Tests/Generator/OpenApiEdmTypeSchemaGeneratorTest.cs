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

        [Fact]
        public void CreateEdmTypeSchemaReturnSchemaForNullableCollectionComplexType()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            ODataContext context = new ODataContext(model);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(
                new EdmCollectionType(new EdmComplexTypeReference(complex, true)));

            // Act
            var schema = context.CreateEdmTypeSchema(collectionType);
            Assert.NotNull(schema);
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            // & Assert
            Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""anyOf"": [
      {
        ""$ref"": ""#/components/schemas/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
      }
    ],
    ""nullable"": true
  }
}".ChangeLineBreaks(), json);
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
    ""type"": ""integer"",
    ""format"": ""int32"",
    ""nullable"": true
  }
}".ChangeLineBreaks(), json);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForEnumType(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmEnumType enumType = model.SchemaElements.OfType<IEdmEnumType>().First(c => c.Name == "PersonGender");
            Assert.NotNull(enumType); // guard
            IEdmEnumTypeReference enumTypeReference = new EdmEnumTypeReference(enumType, isNullable);
            ODataContext context = new ODataContext(model);

            // Act
            var schema = context.CreateEdmTypeSchema(enumTypeReference);

            // & Assert
            Assert.NotNull(schema);
            Assert.Equal(isNullable, schema.Nullable);
            Assert.Null(schema.Reference);
            Assert.NotNull(schema.AnyOf);
            Assert.NotEmpty(schema.AnyOf);
            var anyOf = Assert.Single(schema.AnyOf);
            Assert.NotNull(anyOf.Reference);
            Assert.Equal(ReferenceType.Schema, anyOf.Reference.Type);
            Assert.Equal(enumType.FullTypeName(), anyOf.Reference.Id);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForComplexType(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmComplexType complex = model.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "AirportLocation");
            Assert.NotNull(complex); // guard
            IEdmComplexTypeReference complexTypeReference = new EdmComplexTypeReference(complex, isNullable);
            ODataContext context = new ODataContext(model);

            // Act
            var schema = context.CreateEdmTypeSchema(complexTypeReference);

            // & Assert
            Assert.NotNull(schema);

            if (isNullable)
            {
                Assert.Null(schema.Reference);
                Assert.NotNull(schema.AnyOf);
                Assert.NotEmpty(schema.AnyOf);
                var anyOf = Assert.Single(schema.AnyOf);
                Assert.NotNull(anyOf.Reference);
                Assert.Equal(ReferenceType.Schema, anyOf.Reference.Type);
                Assert.Equal(complex.FullTypeName(), anyOf.Reference.Id);
            }
            else
            {
                Assert.Null(schema.AnyOf);
                Assert.NotNull(schema.Reference);
                Assert.Equal(ReferenceType.Schema, schema.Reference.Type);
                Assert.Equal(complex.FullTypeName(), schema.Reference.Id);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateEdmTypeSchemaReturnSchemaForEntityType(bool isNullable)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            IEdmEntityType entity = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Manager");
            Assert.NotNull(entity); // guard
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entity, isNullable);
            ODataContext context = new ODataContext(model);

            // Act
            var schema = context.CreateEdmTypeSchema(entityTypeReference);

            // & Assert
            Assert.NotNull(schema);

            if (isNullable)
            {
                Assert.Null(schema.Reference);
                Assert.NotNull(schema.AnyOf);
                Assert.NotEmpty(schema.AnyOf);
                var anyOf = Assert.Single(schema.AnyOf);
                Assert.NotNull(anyOf.Reference);
                Assert.Equal(ReferenceType.Schema, anyOf.Reference.Type);
                Assert.Equal(entity.FullTypeName(), anyOf.Reference.Id);
            }
            else
            {
                Assert.Null(schema.AnyOf);
                Assert.NotNull(schema.Reference);
                Assert.Equal(ReferenceType.Schema, schema.Reference.Type);
                Assert.Equal(entity.FullTypeName(), schema.Reference.Id);
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
  ""type"": ""integer"",
  ""format"": ""int32"",
  ""nullable"": true
}".ChangeLineBreaks(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""integer"",
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
                Assert.NotNull(schema.AnyOf);
                Assert.Equal(2, schema.AnyOf.Count);
                Assert.Equal(new[] { "number", "string" }, schema.AnyOf.Select(a => a.Type));
            }
            else
            {
                Assert.Equal("number", schema.Type);
                Assert.Null(schema.AnyOf);
            }

            Assert.Equal(isNullable, schema.Nullable);
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
                Assert.NotNull(schema.AnyOf);
                Assert.Equal(2, schema.AnyOf.Count);
                Assert.Equal(new[] { "integer", "string" }, schema.AnyOf.Select(a => a.Type));
            }
            else
            {
                Assert.Equal("integer", schema.Type);
                Assert.Null(schema.AnyOf);
            }

            Assert.Equal(isNullable, schema.Nullable);
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

            Assert.Equal("double", schema.Format);
            Assert.Equal(isNullable, schema.Nullable);

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

            Assert.Equal("float", schema.Format);
            Assert.Equal(isNullable, schema.Nullable);

            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
        }
        #endregion
    }
}
