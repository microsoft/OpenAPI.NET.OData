// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.IO;
using Microsoft.OpenApi.Writers;
using Xunit;

namespace Microsoft.OpenApi.OData.OpenApiExtensions.Tests;

public class OpenApiEnumFlagsExtensionTests
{
    [Fact]
    public void ExtensionNameMatchesExpected()
    {
        // Arrange
        OpenApiEnumFlagsExtension extension = new();

        // Act
        string name = extension.Name;
        string expectedName = "x-ms-enum-flags";

        // Assert
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void WritesDefaultValues()
    {
        // Arrange
        OpenApiEnumFlagsExtension extension = new();
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.False(extension.IsFlags);
        Assert.Null(extension.Style);
        Assert.Contains("\"isFlags\": false", result);
        Assert.DoesNotContain("\"style\"", result);
    }
    
    [Fact]
    public void WritesAllDefaultValues()
    {
        // Arrange
        OpenApiEnumFlagsExtension extension = new() {
            IsFlags = true
        };
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.True(extension.IsFlags);
        Assert.Null(extension.Style);
        Assert.Contains("\"isFlags\": true", result);
        Assert.DoesNotContain("\"style\":", result);
    }

    [Fact]
    public void WritesAllValues()
    {
        // Arrange
        OpenApiEnumFlagsExtension extension = new() {
            IsFlags = true,
            Style = "simple"
        };
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.True(extension.IsFlags);
        Assert.NotNull(extension.Style);
        Assert.Contains("\"isFlags\": true", result);
        Assert.Contains("\"style\": \"simple\"", result);
    }
}