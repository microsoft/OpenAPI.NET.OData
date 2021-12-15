using System;
using System.IO;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;
using Xunit;

namespace Microsoft.OpenApi.OData.OpenApiExtensions.Tests;

public class OpenApiDeprecationExtensionTests
{
    [Fact]
    public void ExtensionNameMatchesExpected()
    {
        // Arrange
        OpenApiDeprecationExtension extension = new();

        // Act
        string name = extension.Name;
        string expectedName = "x-ms-deprecation";
        Assert.Equal(expectedName, name);

        // Assert
        Assert.Equal(expectedName, name);
    }
    [Fact]
    public void WritesNothingWhenNoValues()
    {
        // Arrange
        OpenApiDeprecationExtension extension = new();
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.Null(extension.Date);
        Assert.Null(extension.RemovalDate);
        Assert.Null(extension.Version);
        Assert.Null(extension.Description);
        Assert.Empty(result);
    }
    [Fact]
    public void WritesAllValues()
    {
        // Arrange
        OpenApiDeprecationExtension extension = new() {
            Date = new DateTime(2020, 1, 1),
            RemovalDate = new DateTime(2021, 1, 1),
            Version = "1.0.0",
            Description = "This is a test"
        };
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.NotNull(extension.Date);
        Assert.NotNull(extension.RemovalDate);
        Assert.NotNull(extension.Version);
        Assert.NotNull(extension.Description);
        Assert.Contains("2021-01-01T00:00:00.000000", result);
        Assert.Contains("removalDate", result);
        Assert.Contains("version", result);
        Assert.Contains("1.0.0", result);
        Assert.Contains("description", result);
        Assert.Contains("This is a test", result);
    }
}