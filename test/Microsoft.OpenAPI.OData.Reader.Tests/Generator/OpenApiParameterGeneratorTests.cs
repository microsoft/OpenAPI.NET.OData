// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
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
            Assert.Collection(parameters,
                item => // $top
                {
                    string json = item.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                    string expected = @"{
  ""name"": ""$top"",
  ""in"": ""query"",
  ""description"": ""Show only the first n items"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {
    ""minimum"": 0,
    ""type"": ""integer""
  },
  ""example"": 50
}";

                    Assert.Equal(expected.ChangeLineBreaks(), json);
                },
                item => // $skip
                {
                    string json = item.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                    string expected = @"{
  ""name"": ""$skip"",
  ""in"": ""query"",
  ""description"": ""Skip the first n items"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {
    ""minimum"": 0,
    ""type"": ""integer""
  }
}";

                    Assert.Equal(expected.ChangeLineBreaks(), json);
                },
                item => // $count
                {
                    string json = item.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                    string expected = @"{
  ""name"": ""$count"",
  ""in"": ""query"",
  ""description"": ""Include count of items"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""boolean""
  }
}";

                    Assert.Equal(expected.ChangeLineBreaks(), json);
                },
                item => // $filter
                {
                    string json = item.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                    string expected = @"{
  ""name"": ""$filter"",
  ""in"": ""query"",
  ""description"": ""Filter items by property values"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""string""
  }
}";

                    Assert.Equal(expected.ChangeLineBreaks(), json);
                },
                item => // $search
                {
                    string json = item.Value.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
                    string expected = @"{
  ""name"": ""$search"",
  ""in"": ""query"",
  ""description"": ""Search items by search phrases"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""string""
  }
}";

                    Assert.Equal(expected.ChangeLineBreaks(), json);
                });
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
style: form
explode: false
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
  ""description"": ""The unique identifier of Customer"",
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
  ""description"": ""The unique identifier of Customer"",
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
  ""description"": ""Property in multi-part unique identifier of Customer"",
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
  ""description"": ""Property in multi-part unique identifier of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  },
  ""x-ms-docs-key-type"": ""Customer""
}";
            Assert.Equal(expected.ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateKeyParametersForAlternateKeyWithSinglePropertyWorks()
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
            var parameters = context.CreateKeyParameters(keySegment);
            var altParameter = parameters.Last();

            // Assert
            Assert.NotNull(parameters);
            Assert.Single(parameters);
            string json = altParameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""name"": ""AltId1"",
  ""in"": ""path"",
  ""description"": ""Alternate key of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateKeyParametersForAlternateKeyWithMultiplePropertiesWorks()
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
            var parameters = context.CreateKeyParameters(keySegment);
            var altParameter1 = parameters.First();
            var altParameter2 = parameters.Last();

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(2, parameters.Count);
            string json1 = altParameter1.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""name"": ""AltId1"",
  ""in"": ""path"",
  ""description"": ""Property in multi-part alternate key of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  }
}".ChangeLineBreaks(), json1);

            string json2 = altParameter2.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            Assert.Equal(@"{
  ""name"": ""AltId2"",
  ""in"": ""path"",
  ""description"": ""Property in multi-part alternate key of Customer"",
  ""required"": true,
  ""schema"": {
    ""type"": ""string"",
    ""nullable"": true
  }
}".ChangeLineBreaks(), json2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateOrderByAndSelectAndExpandParametersWorks(bool useStringArrayForQueryOptionsSchema)
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
            VerifyCreateOrderByParameter(entitySet, context, orderByItemsText);
            VerifyCreateOrderByParameter(singleton, context);
            VerifyCreateOrderByParameter(navigationProperty, context);

            // Select
            string selectItemsText = useStringArrayForQueryOptionsSchema ? null : @"""enum"": [
        ""ID"",
        ""Addresses""
      ],";
            VerifyCreateSelectParameter(entitySet, context, selectItemsText);
            VerifyCreateSelectParameter(singleton, context);
            VerifyCreateSelectParameter(navigationProperty, context);

            // Expand
            string expandItemsText = useStringArrayForQueryOptionsSchema ? null : @"""enum"": [
        ""*"",
        ""Addresses""
      ],";
            VerifyCreateExpandParameter(entitySet, context, expandItemsText);

            string expandItemsDefaultText = useStringArrayForQueryOptionsSchema ? null : @"""enum"": [
        ""*""
      ],";
            VerifyCreateExpandParameter(singleton, context, expandItemsDefaultText);
            VerifyCreateExpandParameter(navigationProperty, context, expandItemsDefaultText);
        }

        private void VerifyCreateOrderByParameter(IEdmElement edmElement, ODataContext context, string orderByItemsText = null)
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

            string json = parameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            string expected = $@"{{
  ""name"": ""$orderby"",
  ""in"": ""query"",
  ""description"": ""Order items by property values"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {{
    ""uniqueItems"": true,
    ""type"": ""array"",
    ""items"": {{
      {itemsText}
    }}
  }}
}}";

            Assert.Equal(expected.ChangeLineBreaks(), json);
        }

        private void VerifyCreateSelectParameter(IEdmElement edmElement, ODataContext context, string selectItemsText = null)
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

            string json = parameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            string expected = $@"{{
  ""name"": ""$select"",
  ""in"": ""query"",
  ""description"": ""Select properties to be returned"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {{
    ""uniqueItems"": true,
    ""type"": ""array"",
    ""items"": {{
      {itemsText}
    }}
  }}
}}";

            Assert.Equal(expected.ChangeLineBreaks(), json);
        }

        private void VerifyCreateExpandParameter(IEdmElement edmElement, ODataContext context, string expandItemsText)
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

            string json = parameter.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);

            string expected = expandItemsText == null
                ? 
                $@"{{
  ""name"": ""$expand"",
  ""in"": ""query"",
  ""description"": ""Expand related entities"",
  ""style"": ""form"",
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
  ""style"": ""form"",
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

            Assert.Equal(expected.ChangeLineBreaks(), json);
        }

        [Fact]
        public void CreateParametersWorks()
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
            IList<OpenApiParameter> parameters1 = context.CreateParameters(function1);
            IList<OpenApiParameter> parameters2 = context.CreateParameters(function2);
            IList<OpenApiParameter> parameters3 = context.CreateParameters(function3);

            // Assert
            Assert.NotNull(parameters1);
            OpenApiParameter parameter1 = Assert.Single(parameters1);

            Assert.NotNull(parameters2);
            OpenApiParameter parameter2 = Assert.Single(parameters2);

            Assert.NotNull(parameters3);
            Assert.Equal(4, parameters3.Count);
            OpenApiParameter parameter3 = parameters3.First();

            string json1 = parameter1.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            string expectedPayload1 = $@"{{
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
}}";

            string json2 = parameter2.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            string expectedPayload2 = $@"{{
  ""name"": ""resource"",
  ""in"": ""path"",
  ""required"": true,
  ""schema"": {{
    ""type"": ""string"",
    ""nullable"": true
  }}
}}";

            string json3 = parameter3.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
            string expectedPayload3 = $@"{{
  ""name"": ""directoryScopeId"",
  ""in"": ""query"",
  ""schema"": {{
    ""type"": ""string"",
    ""nullable"": true
  }}
}}";

            Assert.Equal(expectedPayload1.ChangeLineBreaks(), json1);
            Assert.Equal(expectedPayload2.ChangeLineBreaks(), json2);
            Assert.Equal(expectedPayload3.ChangeLineBreaks(), json3);
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