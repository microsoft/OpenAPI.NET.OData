// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Moq;
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
            var mockModel = new Mock<IEdmModel>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.AddParametersToDocument(new()));
            Assert.Throws<ArgumentNullException>("document", () => new ODataContext(mockModel).AddParametersToDocument(null));
        }

        [Fact]
        public async Task CreateParametersReturnsCreatedParameters()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            OpenApiDocument openApiDocument = new ();

            // Act
            context.AddParametersToDocument(openApiDocument);
            var parameters = openApiDocument.Components.Parameters;

            // Assert
            Assert.NotNull(parameters);
            Assert.NotEmpty(parameters);
            Assert.Equal(5, parameters.Count);
            Assert.Equal(new[] { "top", "skip", "count", "filter", "search" },
                parameters.Select(p => p.Key));
            var expectedTop = JsonNode.Parse(@"{
  ""name"": ""$top"",
  ""in"": ""query"",
  ""description"": ""Show only the first n items"",
  ""explode"": false,
  ""schema"": {
    ""minimum"": 0,
    ""type"": ""number"",
    ""format"": ""int64""
  },
  ""example"": 50
}");
            var expectedSkip = JsonNode.Parse(@"{
  ""name"": ""$skip"",
  ""in"": ""query"",
  ""description"": ""Skip the first n items"",
  ""explode"": false,
  ""schema"": {
    ""minimum"": 0,
    ""type"": ""number"",
    ""format"": ""int64""
  }
}");
            var expectedCount = JsonNode.Parse(@"{
  ""name"": ""$count"",
  ""in"": ""query"",
  ""description"": ""Include count of items"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""boolean""
  }
}");
            var expectedFilter = JsonNode.Parse(@"{
  ""name"": ""$filter"",
  ""in"": ""query"",
  ""description"": ""Filter items by property values"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""string""
  }
}");
            var expectedSearch = JsonNode.Parse(@"{
  ""name"": ""$search"",
  ""in"": ""query"",
  ""description"": ""Search items by search phrases"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""string""
  }
}");
            var parametersAsRawJson = await Task.WhenAll(parameters.Select(p => p.Value.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0)));
            var parametersAsJson = parametersAsRawJson.Select(x => JsonNode.Parse(x)).ToArray();
            Assert.Contains(parametersAsJson, p => JsonNode.DeepEquals(expectedTop, p));
            Assert.Contains(parametersAsJson, p => JsonNode.DeepEquals(expectedSkip, p));
            Assert.Contains(parametersAsJson, p => JsonNode.DeepEquals(expectedCount, p));
            Assert.Contains(parametersAsJson, p => JsonNode.DeepEquals(expectedFilter, p));
            Assert.Contains(parametersAsJson, p => JsonNode.DeepEquals(expectedSearch, p));
        }

        [Fact]
        public async Task CanSerializeAsYamlFromTheCreatedParameters()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataContext context = new ODataContext(model);
            OpenApiDocument openApiDocument = new ();

            // Act
            context.AddParametersToDocument(openApiDocument);
            var parameters = openApiDocument.Components.Parameters;

            // Assert
            Assert.Contains("skip", parameters.Select(p => p.Key));
            var skip = parameters.First(c => c.Key == "skip").Value;

            string yaml = await skip.SerializeAsYamlAsync(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(
@"name: $skip
in: query
description: Skip the first n items
explode: false
schema:
  minimum: 0
  type: number
  format: int64
".ChangeLineBreaks(), yaml);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateKeyParametersForSingleKeyWorks(bool prefix)
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
            var parameters = context.CreateKeyParameters(keySegment, new());

            // Assert
            Assert.NotNull(parameters);
            var parameter = Assert.Single(parameters);

            var json = JsonNode.Parse(await parameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            JsonNode expected;

            if (prefix)
            {
                expected = JsonNode.Parse(@"{
  ""name"": ""Customer-Id"",
  ""in"": ""path"",
  ""description"": ""The unique identifier of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}");
            }
            else
            {
                expected = JsonNode.Parse(@"{
  ""name"": ""Id"",
  ""in"": ""path"",
  ""description"": ""The unique identifier of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}");
            }

            Assert.True(JsonNode.DeepEquals(expected, json));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateKeyParametersForCompositeKeyWorks(bool prefix)
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
            var parameters = context.CreateKeyParameters(keySegment, new());

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(2, parameters.Count);

            // 1st
            var parameter = parameters.First();
            var json = JsonNode.Parse(await parameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expected = JsonNode.Parse(@"{
  ""name"": ""firstName"",
  ""in"": ""path"",
  ""description"": ""Property in multi-part unique identifier of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}");
            Assert.True(JsonNode.DeepEquals(expected, json));

            // 2nd
            parameter = parameters.Last();
            json = JsonNode.Parse(await parameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            expected = JsonNode.Parse(@"{
  ""name"": ""lastName"",
  ""in"": ""path"",
  ""description"": ""Property in multi-part unique identifier of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}");
            Assert.True(JsonNode.DeepEquals(expected, json));
        }

        [Fact]
        public async Task CreateKeyParametersForAlternateKeyWithSinglePropertyWorks()
        {
            // Arrange
            EdmModel model = new();
            EdmEntityType customer = new("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String));

            IEdmProperty alternateId1 = customer.AddStructuralProperty("AlternateId1", EdmPrimitiveTypeKind.String);
            IEdmProperty alternateId2 = customer.AddStructuralProperty("AlternateId2", EdmPrimitiveTypeKind.String);
            model.AddAlternateKeyAnnotation(customer, new Dictionary<string, IEdmProperty> { { "AltId1", alternateId1 } });
            model.AddAlternateKeyAnnotation(customer, new Dictionary<string, IEdmProperty> { { "AltId2", alternateId2 } });
            IDictionary<string, string> keyMappings = new Dictionary<string, string> { { "AltId1", "AltId1" } };

            model.AddElement(customer);
            ODataContext context = new(model);
            ODataKeySegment keySegment = new(customer, keyMappings)
            {
                IsAlternateKey = true
            };

            // Act
            var parameters = context.CreateKeyParameters(keySegment, new());
            var altParameter = parameters.Last();

            // Assert
            Assert.NotNull(parameters);
            Assert.Single(parameters);
            var json = JsonNode.Parse(await altParameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expected = JsonNode.Parse(@"{
  ""name"": ""AltId1"",
  ""in"": ""path"",
  ""description"": ""Alternate key of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  }
}");
            Assert.True(JsonNode.DeepEquals(expected, json));
        }

        [Fact]
        public async Task CreateKeyParametersForAlternateKeyWithMultiplePropertiesWorks()
        {
            // Arrange
            EdmModel model = new();
            EdmEntityType customer = new("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String));

            IEdmProperty alternateId1 = customer.AddStructuralProperty("AlternateId1", EdmPrimitiveTypeKind.String);
            IEdmProperty alternateId2 = customer.AddStructuralProperty("AlternateId2", EdmPrimitiveTypeKind.String);
            model.AddAlternateKeyAnnotation(customer,
                new Dictionary<string, IEdmProperty>
                {
                    { "AltId1", alternateId1 },
                    { "AltId2", alternateId2 }
                });

            IDictionary<string, string> keyMappings = new Dictionary<string, string> { { "AltId1", "AltId1" }, { "AltId2", "AltId2" } };

            model.AddElement(customer);
            ODataContext context = new(model);
            ODataKeySegment keySegment = new(customer, keyMappings)
            {
                IsAlternateKey = true
            };

            // Act
            var parameters = context.CreateKeyParameters(keySegment, new());
            var altParameter1 = parameters.First();
            var altParameter2 = parameters.Last();

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(2, parameters.Count);
            var json1 = JsonNode.Parse(await altParameter1.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expected1 = JsonNode.Parse(@"{
  ""name"": ""AltId1"",
  ""in"": ""path"",
  ""description"": ""Property in multi-part alternate key of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  }
}");
            Assert.True(JsonNode.DeepEquals(expected1, json1));

            var json2 = JsonNode.Parse(await altParameter2.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expected2 = JsonNode.Parse(@"{
  ""name"": ""AltId2"",
  ""in"": ""path"",
  ""description"": ""Property in multi-part alternate key of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  }
}");
            Assert.True(JsonNode.DeepEquals(expected2, json2));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateOrderByAndSelectAndExpandParametersWorks(bool useStringArrayForQueryOptionsSchema)
        {
            // Arrange
            IEdmModel model = GetEdmModel();
            ODataContext context = new(model,
                new OpenApiConvertSettings() 
                { 
                    UseStringArrayForQueryOptionsSchema = useStringArrayForQueryOptionsSchema 
                });
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            IEdmSingleton singleton = model.EntityContainer.FindSingleton("Catalog");
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            IEdmNavigationProperty navigationProperty = entityType.DeclaredNavigationProperties().First(c => c.Name == "Addresses");

            // Act & Assert
            // OrderBy
            string orderByItemsText = useStringArrayForQueryOptionsSchema ? null :  @"""enum"": [
        ""ID"",
        ""ID desc""
      ],";
            await VerifyCreateOrderByParameter(entitySet, context, orderByItemsText);
            await VerifyCreateOrderByParameter(singleton, context);
            await VerifyCreateOrderByParameter(navigationProperty, context);

            // Select
            string selectItemsText = useStringArrayForQueryOptionsSchema ? null : @"""enum"": [
        ""ID"",
        ""Addresses""
      ],";
            await VerifyCreateSelectParameter(entitySet, context, selectItemsText);
            await VerifyCreateSelectParameter(singleton, context);
            await VerifyCreateSelectParameter(navigationProperty, context);

            // Expand
            string expandItemsText = useStringArrayForQueryOptionsSchema ? null : @"""enum"": [
        ""*"",
        ""Addresses""
      ],";
            await VerifyCreateExpandParameter(entitySet, context, expandItemsText);

            string expandItemsDefaultText = useStringArrayForQueryOptionsSchema ? null : @"""enum"": [
        ""*""
      ],";
            await VerifyCreateExpandParameter(singleton, context, expandItemsDefaultText);
            await VerifyCreateExpandParameter(navigationProperty, context, expandItemsDefaultText);
        }

        private static async Task VerifyCreateOrderByParameter(IEdmElement edmElement, ODataContext context, string orderByItemsText = null)
        {
            // Arrange & Act
            OpenApiParameter parameter;
            switch (edmElement)
            {
                case IEdmEntitySet entitySet:
                    parameter = context.CreateOrderBy(entitySet);
                    break;
                case IEdmSingleton singleton:
                    parameter = context.CreateOrderBy(singleton);
                    break;
                case IEdmNavigationProperty navigationProperty:
                    parameter = context.CreateOrderBy(navigationProperty);
                    break;
                default:
                    return;
            }

            string itemsText = orderByItemsText == null
                ? @"""type"": ""string"""
                : $@"{orderByItemsText}
      ""type"": ""string""";

            // Assert
            Assert.NotNull(parameter);

            var json = JsonNode.Parse(await parameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expectedJson = JsonNode.Parse($@"{{
  ""name"": ""$orderby"",
  ""in"": ""query"",
  ""description"": ""Order items by property values"",
  ""explode"": false,
  ""schema"": {{
    ""uniqueItems"": true,
    ""type"": ""array"",
    ""items"": {{
      {itemsText}
    }}
  }}
}}");

            Assert.True(JsonNode.DeepEquals(expectedJson, json));
        }

        private static async Task VerifyCreateSelectParameter(IEdmElement edmElement, ODataContext context, string selectItemsText = null)
        {
            // Arrange & Act
            OpenApiParameter parameter;
            switch (edmElement)
            {
                case IEdmEntitySet entitySet:
                    parameter = context.CreateSelect(entitySet);
                    break;
                case IEdmSingleton singleton:
                    parameter = context.CreateSelect(singleton);
                    break;
                case IEdmNavigationProperty navigationProperty:
                    parameter = context.CreateSelect(navigationProperty);
                    break;
                default:
                    return;
            }

            string itemsText = selectItemsText == null
                ? @"""type"": ""string"""
                : $@"{selectItemsText}
      ""type"": ""string""";

            // Assert
            Assert.NotNull(parameter);

            var json = JsonNode.Parse(await parameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expectedJson = JsonNode.Parse($@"{{
  ""name"": ""$select"",
  ""in"": ""query"",
  ""description"": ""Select properties to be returned"",
  ""explode"": false,
  ""schema"": {{
    ""uniqueItems"": true,
    ""type"": ""array"",
    ""items"": {{
      {itemsText}
    }}
  }}
}}");
            Assert.True(JsonNode.DeepEquals(expectedJson, json));
        }

        private static async Task VerifyCreateExpandParameter(IEdmElement edmElement, ODataContext context, string expandItemsText)
        {
            // Arrange & Act
            OpenApiParameter parameter;
            switch (edmElement)
            {
                case IEdmEntitySet entitySet:
                    parameter = context.CreateExpand(entitySet);
                    break;
                case IEdmSingleton singleton:
                    parameter = context.CreateExpand(singleton);
                    break;
                case IEdmNavigationProperty navigationProperty:
                    parameter = context.CreateExpand(navigationProperty);
                    break;
                default:
                    return;
            }

            // Assert
            Assert.NotNull(parameter);

            var json = JsonNode.Parse(await parameter.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expectedJson = expandItemsText == null
                ? 
                $@"{{
  ""name"": ""$expand"",
  ""in"": ""query"",
  ""description"": ""Expand related entities"",
  ""explode"": false,
  ""schema"": {{
    ""uniqueItems"": true,
    ""type"": ""array"",
    ""items"": {{
      ""type"": ""string""
    }}
  }}
}}"
                :
                $@"{{
  ""name"": ""$expand"",
  ""in"": ""query"",
  ""description"": ""Expand related entities"",
  ""explode"": false,
  ""schema"": {{
    ""uniqueItems"": true,
    ""type"": ""array"",
    ""items"": {{
      {expandItemsText}
      ""type"": ""string""
    }}
  }}
}}";
            var expectedJsonNode = JsonNode.Parse(expectedJson);
            Assert.True(JsonNode.DeepEquals(expectedJsonNode, json));
        }

        [Fact]
        public async Task CreateParametersWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new(model);
            IEdmSingleton deviceMgmt = model.EntityContainer.FindSingleton("deviceManagement");
            Assert.NotNull(deviceMgmt);

            IEdmFunction function1 = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "getRoleScopeTagsByIds");
            Assert.NotNull(function1);

            IEdmFunction function2 = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "getRoleScopeTagsByResource");
            Assert.NotNull(function2);

            IEdmFunction function3 = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "roleScheduleInstances");
            Assert.NotNull(function3);

            // Act
            var parameters1 = context.CreateParameters(function1, new());
            var parameters2 = context.CreateParameters(function2, new());
            var parameters3 = context.CreateParameters(function3, new());

            // Assert
            Assert.NotNull(parameters1);
            var parameter1 = Assert.Single(parameters1);

            Assert.NotNull(parameters2);
            var parameter2 = Assert.Single(parameters2);

            Assert.NotNull(parameters3);
            Assert.Equal(4, parameters3.Count);
            var parameter3 = parameters3.First();

            var json1 = JsonNode.Parse(await parameter1.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expectedPayload1 = JsonNode.Parse($@"{{
  ""name"": ""ids"",
  ""in"": ""path"",
  ""description"": ""The URL-encoded JSON object"",
  ""required"": true,
  ""content"": {{
    ""application/json"": {{
      ""schema"": {{
        ""type"": ""array"",
        ""items"": {{
          ""type"": ""string""
        }}
      }}
    }}
  }}
}}");
            Assert.True(JsonNode.DeepEquals(expectedPayload1, json1));

            var json2 = JsonNode.Parse(await parameter2.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expectedPayload2 = JsonNode.Parse($@"{{
  ""name"": ""resource"",
  ""in"": ""path"",
  ""required"": true,
  ""schema"": {{
    ""type"": ""string"",
    ""nullable"": true
  }}
}}");
            Assert.True(JsonNode.DeepEquals(expectedPayload2, json2));

            var json3 = JsonNode.Parse(await parameter3.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0));
            var expectedPayload3 = JsonNode.Parse($@"{{
  ""name"": ""directoryScopeId"",
  ""in"": ""query"",
  ""schema"": {{
    ""type"": ""string"",
    ""nullable"": true
  }}
}}");
            Assert.True(JsonNode.DeepEquals(expectedPayload3, json3));
        }

        public static IEdmModel GetEdmModel()
        {
            const string modelText = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Customer"">
        <Key>
          <PropertyRef Name=""ID"" />
        </Key>
        <Property Name=""ID"" Type=""Edm.Int32"" Nullable=""false"" />
        <NavigationProperty Name=""Addresses"" Type=""Collection(NS.Address)"" />
      </EntityType>
      <EntityContainer Name =""Default"">
        <EntitySet Name=""Customers"" EntityType=""NS.Customer"" />
        <Singleton Name=""Catalog"" Type=""NS.Catalog"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out IEdmModel model, out _);
            Assert.True(result);
            return model;
        }
    }
}