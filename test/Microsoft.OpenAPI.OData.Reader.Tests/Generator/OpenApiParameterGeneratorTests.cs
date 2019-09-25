// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiParameterGeneratorTest
    {
        [Fact]
        public void CreateParametersThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateParameters());
        }

        [Fact]
        public void CreateParametersReturnsCreatedParameters()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);

            // Act
            var parameters = context.CreateParameters();

            // Assert
            Assert.NotNull(parameters);
            Assert.NotEmpty(parameters);
            Assert.Equal(5, parameters.Count);
            Assert.Equal(new[] { "top", "skip", "count", "filter", "search" },
                parameters.Select(p => p.Key));
        }

        [Fact]
        public void CanSeralizeAsYamlFromTheCreatedParameters()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);

            // Act
            var parameters = context.CreateParameters();

            // Assert
            Assert.Contains("skip", parameters.Select(p => p.Key));
            var skip = parameters.First(c => c.Key == "skip").Value;

            string yaml = skip.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(
@"name: $skip
in: query
description: Skip the first n items
schema:
  minimum: 0
  type: integer
".ChangeLineBreaks(), yaml);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateKeyParametersForSingleKeyWorks(bool prefix)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String));
            model.AddElement(customer);
            OpenApiConvertSettings setting = new OpenApiConvertSettings
            {
                PrefixEntityTypeNameBeforeKey = prefix
            };
            ODataContext context = new ODataContext(model, setting);
            ODataKeySegment keySegment = new ODataKeySegment(customer);

            // Act
            var parameters = context.CreateKeyParameters(keySegment);

            // Assert
            Assert.NotNull(parameters);
            var parameter = Assert.Single(parameters);

            string json = parameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            string expected;

            if (prefix)
            {
                expected = @"{
  ""name"": ""Customer-Id"",
  ""in"": ""path"",
  ""description"": ""key: Customer-Id of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}";
            }
            else
            {
                expected = @"{
  ""name"": ""Id"",
  ""in"": ""path"",
  ""description"": ""key: Id of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}";
            }

            Assert.Equal(expected.ChangeLineBreaks(), json);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateKeyParametersForCompositeKeyWorks(bool prefix)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String));
            customer.AddKeys(customer.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String));
            model.AddElement(customer);
            OpenApiConvertSettings setting = new OpenApiConvertSettings
            {
                PrefixEntityTypeNameBeforeKey = prefix
            };
            ODataContext context = new ODataContext(model, setting);
            ODataKeySegment keySegment = new ODataKeySegment(customer);

            // Act
            var parameters = context.CreateKeyParameters(keySegment);

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(2, parameters.Count);

            // 1st
            var parameter = parameters.First();
            string json = parameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            string expected = @"{
  ""name"": ""firstName"",
  ""in"": ""path"",
  ""description"": ""key: firstName of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}";
            Assert.Equal(expected.ChangeLineBreaks(), json);

            // 2nd
            parameter = parameters.Last();
            json = parameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            expected = @"{
  ""name"": ""lastName"",
  ""in"": ""path"",
  ""description"": ""key: lastName of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}";
            Assert.Equal(expected.ChangeLineBreaks(), json);
        }
    }
}