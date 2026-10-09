// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Hidi.Extensions;
using Microsoft.OpenApi.Hidi.Utilities;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests
{
    public sealed class UtilityRegressionTests
    {
        [Theory]
        [InlineData("2.0", OpenApiSpecVersion.OpenApi2_0)]
        [InlineData("2.1", OpenApiSpecVersion.OpenApi2_0)]
        [InlineData("3.0", OpenApiSpecVersion.OpenApi3_0)]
        [InlineData("3.1", OpenApiSpecVersion.OpenApi3_1)]
        [InlineData("3.1.1", OpenApiSpecVersion.OpenApi3_1)]
        [InlineData("3.2", OpenApiSpecVersion.OpenApi3_1)]
        [InlineData("4.0", OpenApiSpecVersion.OpenApi3_1)]
        public void ParseSpecificationVersionPreservesSupportedAndFallbackVersions(string value, OpenApiSpecVersion expected)
        {
            Assert.Equal(expected, OpenApiSpecVersionHelper.TryParseOpenApiSpecVersion(value));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ParseSpecificationVersionRejectsMissingVersion(string? value)
        {
            var error = Assert.Throws<InvalidOperationException>(() => OpenApiSpecVersionHelper.TryParseOpenApiSpecVersion(value!));
            Assert.Equal("Please provide a version", error.Message);
        }

        [Theory]
        [InlineData("invalid.0")]
        [InlineData("3.invalid")]
        public void ParseSpecificationVersionRejectsNonnumericSegments(string value)
        {
            var error = Assert.Throws<InvalidOperationException>(() => OpenApiSpecVersionHelper.TryParseOpenApiSpecVersion(value));
            Assert.Contains("Invalid version format", error.Message, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(" ", null, true)]
        [InlineData(null, "value", false)]
        [InlineData(" ", "value", false)]
        [InlineData("value", null, false)]
        [InlineData("VALUE", "value", true)]
        [InlineData("value", "different", false)]
        public void StringEqualityHandlesMissingValuesAndIgnoresCase(string? value, string? search, bool expected)
        {
            Assert.Equal(expected, value.IsEquals(search));
        }

        [Fact]
        public void StringEqualityHonorsExplicitComparison()
        {
            Assert.False("VALUE".IsEquals("value", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void SplitByCharReturnsNoItemsForWhitespace(string value)
        {
            Assert.Empty(value.SplitByChar(','));
        }

        [Fact]
        public void SplitByCharOmitsEmptyItemsWithoutTrimmingValues()
        {
            Assert.Equal<string>(["one", " two", "three"], "one,, two,three,".SplitByChar(','));
        }

        [Fact]
        public void ExtensionLookupReturnsOnlyStringValues()
        {
            var extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["text"] = new JsonNodeExtension("function"),
                ["number"] = new JsonNodeExtension(42),
                ["object"] = new JsonNodeExtension(new JsonObject())
            };

            Assert.Equal("function", extensions.GetExtension("text"));
            Assert.Empty(extensions.GetExtension("number"));
            Assert.Empty(extensions.GetExtension("object"));
            Assert.Empty(extensions.GetExtension("missing"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("v2")]
        public void ConversionSettingsUseMetadataVersionWhenNotConfigured(string? metadataVersion)
        {
            var config = new ConfigurationBuilder().Build();
            var expected = new Microsoft.OpenApi.OData.OpenApiConvertSettings();
            var actual = SettingsUtilities.GetOpenApiConvertSettings(config, metadataVersion);

            Assert.Equal(string.IsNullOrEmpty(metadataVersion) ? expected.SemVerVersion : metadataVersion, actual.SemVerVersion);
        }

        [Fact]
        public void ConfiguredConversionSettingsOverrideMetadataDefaults()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenApiConvertSettings:SemVerVersion"] = "configured",
                ["OpenApiConvertSettings:EnablePagination"] = "false"
            }).Build();

            var settings = SettingsUtilities.GetOpenApiConvertSettings(config, "metadata");

            Assert.Equal("configured", settings.SemVerVersion);
            Assert.False(settings.EnablePagination);
        }

        [Fact]
        public void ConversionSettingsRejectMissingConfiguration()
        {
            Assert.Throws<ArgumentNullException>(() => SettingsUtilities.GetOpenApiConvertSettings(null!, "v2"));
        }

        [Fact]
        public void StatisticsCountEverySupportedElementAndFormatTheReport()
        {
            var visitor = new StatsVisitor();
            IOpenApiParameter parameter = new OpenApiParameter();
            IOpenApiSchema schema = new OpenApiSchema();
            IOpenApiPathItem path = new OpenApiPathItem();
            IOpenApiRequestBody requestBody = new OpenApiRequestBody();
            IOpenApiLink link = new OpenApiLink();
            IOpenApiCallback callback = new OpenApiCallback();
            visitor.Visit(parameter);
            visitor.Visit(schema);
            visitor.Visit(new Dictionary<string, IOpenApiHeader>());
            visitor.Visit(path);
            visitor.Visit(requestBody);
            visitor.Visit(new OpenApiResponses());
            visitor.Visit(new OpenApiOperation());
            visitor.Visit(link);
            visitor.Visit(callback);
            visitor.Visit(parameter);

            Assert.Equal(2, visitor.ParameterCount);
            Assert.Equal(1, visitor.HeaderCount);
            Assert.Equal(1, visitor.PathItemCount);
            Assert.Equal(1, visitor.OperationCount);
            Assert.Equal(1, visitor.RequestBodyCount);
            Assert.Equal(1, visitor.ResponseCount);
            Assert.Equal(1, visitor.LinkCount);
            Assert.Equal(1, visitor.CallbackCount);
            Assert.Equal(1, visitor.SchemaCount);
            var expected = string.Join(Environment.NewLine,
            [
                "Path Items: 1", "Operations: 1", "Parameters: 2", "Request Bodies: 1",
                "Responses: 1", "Links: 1", "Callbacks: 1", "Schemas: 1", ""
            ]);
            Assert.Equal(expected, visitor.GetStatisticsReport());
        }
    }
}
