#nullable enable
using System;
using Microsoft.OpenApi.Hidi;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests;

public class OpenApiSpecVersionHelperTests
{
    [Theory]
    [InlineData("2.0", OpenApiSpecVersion.OpenApi2_0)]
    [InlineData("2.1", OpenApiSpecVersion.OpenApi2_0)]
    [InlineData("3.0", OpenApiSpecVersion.OpenApi3_0)]
    [InlineData("3.1", OpenApiSpecVersion.OpenApi3_1)]
    [InlineData("3.1.1", OpenApiSpecVersion.OpenApi3_1)]
    [InlineData("3.2", OpenApiSpecVersion.OpenApi3_2)]
    [InlineData("4.0", OpenApiSpecVersion.OpenApi3_2)]
    public void TryParseOpenApiSpecVersionReturnsExpectedVersion(string version, OpenApiSpecVersion expectedVersion)
    {
        var result = OpenApiSpecVersionHelper.TryParseOpenApiSpecVersion(version);

        Assert.Equal(expectedVersion, result);
    }

    [Theory]
    [InlineData(null, "Please provide a version")]
    [InlineData("", "Please provide a version")]
    [InlineData("abc", "Invalid version format")]
    [InlineData("invalid.0", "Invalid version format")]
    [InlineData("3.invalid", "Invalid version format")]
    public void TryParseOpenApiSpecVersionThrowsForInvalidValues(string? version, string expectedMessage)
    {
        var error = Assert.Throws<InvalidOperationException>(() => OpenApiSpecVersionHelper.TryParseOpenApiSpecVersion(version!));

        Assert.Contains(expectedMessage, error.Message, StringComparison.Ordinal);
    }
}
