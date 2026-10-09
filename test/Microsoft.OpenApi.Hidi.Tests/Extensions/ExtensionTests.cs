// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Nodes;
using Microsoft.OpenApi.Hidi.Extensions;
using Moq;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests
{
    public class ExtensionTests
    {
        [Theory]
        [InlineData(null, null, true)]
        [InlineData("", " ", true)]
        [InlineData("\t", "value", false)]
        [InlineData(null, "value", false)]
        [InlineData("Hidi", "HIDI", true)]
        [InlineData("Hidi", "different", false)]
        [InlineData("Hidi", null, false)]
        public void CompareStringsUsingDefaultCaseInsensitiveComparison(string? target, string? searchValue, bool expected)
        {
            Assert.Equal(expected, target.IsEquals(searchValue));
        }

        [Fact]
        public void RespectExplicitCaseSensitiveComparison()
        {
            Assert.False("Hidi".IsEquals("HIDI", StringComparison.Ordinal));
            Assert.True("Hidi".IsEquals("Hidi", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t")]
        public void SplitEmptyStringsIntoEmptyLists(string? target)
        {
            Assert.Empty(target!.SplitByChar(','));
        }

        [Fact]
        public void SplitWithoutEmptyEntriesWhilePreservingWhitespaceAndDuplicates()
        {
            Assert.Equal<string>(["first", " second", "first"], "first,, second,first,".SplitByChar(','));
        }

        [Fact]
        public void ReadStringExtensionValue()
        {
            var extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-value"] = new JsonNodeExtension("value")
            };

            Assert.Equal("value", extensions.GetExtension("x-value"));
        }

        [Fact]
        public void ReturnEmptyStringForMissingExtension()
        {
            var extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-value"] = new JsonNodeExtension("value")
            };

            Assert.Equal(string.Empty, extensions.GetExtension("x-missing"));
        }

        [Fact]
        public void ReturnEmptyStringForNonJsonExtension()
        {
            var extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-value"] = Mock.Of<IOpenApiExtension>()
            };

            Assert.Equal(string.Empty, extensions.GetExtension("x-value"));
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("[]")]
        [InlineData("42")]
        [InlineData("true")]
        public void ReturnEmptyStringForNonStringJsonExtension(string json)
        {
            var extensions = new Dictionary<string, IOpenApiExtension>
            {
                ["x-value"] = new JsonNodeExtension(JsonNode.Parse(json)!)
            };

            Assert.Equal(string.Empty, extensions.GetExtension("x-value"));
        }
    }
}
