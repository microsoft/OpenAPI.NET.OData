// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class RecordExpressionExtensionsTests
    {
        [Fact]
        public void GetIntegerWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmIntegerConstant(42)));

            // Act
            long? actual = record.GetInteger("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(42, actual.Value);
        }

        [Fact]
        public void GetStringWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmStringConstant("test")));

            // Act
            string actual = record.GetString("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal("test", actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetBooleanWorks(bool expected)
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmBooleanConstant(expected)));

            // Act
            bool? actual = record.GetBoolean("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual.Value);
        }

        [Fact]
        public void GetEnumWorks()
        {
            // Arrange
            IEdmEnumType enumType = new EdmEnumType("NS", "Color");
            EdmEnumMember member = new EdmEnumMember(enumType, "Red", new EdmEnumMemberValue(2));
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmEnumMemberExpression(member)));

            // Act
            Color? actual = record.GetEnum<Color>("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(Color.Red, actual.Value);
        }

        private enum Color
        {
            Red
        }

        [Fact]
        public void GetPropertyPathWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmPropertyPathExpression("abc/xyz")));

            // Act
            string actual = record.GetPropertyPath("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal("abc/xyz", actual);
        }

        [Fact]
        public void GetCollectionPropertyPathWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmCollectionExpression(
                    new EdmPropertyPathExpression("abc/xyz"),
                    new EdmPropertyPathExpression("123"))));

            // Act
            IList<string> actual = record.GetCollectionPropertyPath("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Count);
            Assert.Equal(new[] { "abc/xyz", "123" }, actual);
        }

        [Fact]
        public void GetRecordWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmRecordExpression(
                    new EdmPropertyConstructor("Scope", new EdmStringConstant("scope name")),
                    new EdmPropertyConstructor("RestrictedProperties", new EdmStringConstant("*")))));

            // Act
            ScopeType actual = record.GetRecord<ScopeType>("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal("scope name", actual.Scope);
            Assert.Equal("*", actual.RestrictedProperties);
        }

        [Fact]
        public void GetCollectionForStringWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmCollectionExpression(
                    new EdmStringConstant("abc"), new EdmStringConstant("xyz"))));

            // Act
            IList<string> actual = record.GetCollection("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Count);
            Assert.Equal(new[] { "abc", "xyz" }, actual);
        }

        [Fact]
        public void GetCollectionForRecordWorks()
        {
            // Arrange
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("prop", new EdmCollectionExpression(
                    new EdmRecordExpression(
                        new EdmPropertyConstructor("Scope", new EdmStringConstant("scope1")),
                        new EdmPropertyConstructor("RestrictedProperties", new EdmStringConstant("restrictedProperties1"))),
                    new EdmRecordExpression(
                        new EdmPropertyConstructor("Scope", new EdmStringConstant("scope2")),
                        new EdmPropertyConstructor("RestrictedProperties", new EdmStringConstant("restrictedProperties2"))))));

            // Act
            IList<ScopeType> actual = record.GetCollection<ScopeType>("prop");

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(2, actual.Count);

            for(int i = 1; i <= actual.Count; i++)
            {
                Assert.Equal("scope" + i, actual[i-1].Scope);
                Assert.Equal("restrictedProperties" + i, actual[i-1].RestrictedProperties);
            }
        }
    }
}
