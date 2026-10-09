using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Hidi.Utilities;
using Microsoft.OpenApi.OData;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests;

public class SettingsUtilitiesTests
{
    [Fact]
    public void GetOpenApiConvertSettingsThrowsWhenConfigurationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => SettingsUtilities.GetOpenApiConvertSettings(null!, null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2.1")]
    public void GetOpenApiConvertSettingsUsesMetadataVersionWhenSectionIsMissing(string? metadataVersion)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        var settings = SettingsUtilities.GetOpenApiConvertSettings(configuration, metadataVersion);

        var expected = string.IsNullOrEmpty(metadataVersion) ? new OpenApiConvertSettings().SemVerVersion : metadataVersion;
        Assert.Equal(expected, settings.SemVerVersion);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetOpenApiConvertSettingsBindsConfiguredValuesOverMetadataVersion(bool enablePagination)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(OpenApiConvertSettings)}:{nameof(OpenApiConvertSettings.SemVerVersion)}"] = "3.0",
                [$"{nameof(OpenApiConvertSettings)}:{nameof(OpenApiConvertSettings.EnablePagination)}"] = enablePagination.ToString()
            })
            .Build();

        var settings = SettingsUtilities.GetOpenApiConvertSettings(configuration, "2.1");

        Assert.Equal("3.0", settings.SemVerVersion);
        Assert.Equal(enablePagination, settings.EnablePagination);
    }
}
