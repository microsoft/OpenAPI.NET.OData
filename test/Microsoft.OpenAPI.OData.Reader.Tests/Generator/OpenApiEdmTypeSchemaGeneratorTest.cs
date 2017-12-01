// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

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
}".Replace(), json);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            // & Assert
            Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""#/components/schemas/Microsoft.OData.Service.Sample.TrippinInMemory.Models.AirportLocation""
  }
}".Replace(), json);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            // & Assert
            Assert.Equal(@"{
  ""type"": ""array"",
  ""items"": {
    ""type"": ""string""
  }
}".Replace(), json);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

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
}".Replace(), json);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            // & Assert
            if (isNullable)
            {
                Assert.Equal(@"{
  ""type"": ""string"",
  ""nullable"": true
}".Replace(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""type"": ""string""
}".Replace(), json);
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
            string json = schema.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0_0);

            // & Assert
            if (isNullable)
            {
                Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""integer"",
  ""format"": ""int32"",
  ""nullable"": true
}".Replace(), json);
            }
            else
            {
                Assert.Equal(@"{
  ""maximum"": 2147483647,
  ""minimum"": -2147483648,
  ""type"": ""integer"",
  ""format"": ""int32""
}".Replace(), json);
            }
        }
    }
}
