// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Writers;
using Xunit;

namespace Microsoft.OpenApi.OData.OpenApiExtensions.Tests;

public class OpenApiEnumValuesDescriptionExtensionTexts
{
    [Fact]
    public void ExtensionNameMatchesExpected()
    {
        // Arrange
        OpenApiEnumValuesDescriptionExtension extension = new();

        // Act
        string name = extension.Name;
        string expectedName = "x-ms-enum";

        // Assert
        Assert.Equal(expectedName, name);
    }

    [Fact]
    public void WritesNothingWhenNoValues()
    {
        // Arrange
        OpenApiEnumValuesDescriptionExtension extension = new();
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.Null(extension.EnumName);
        Assert.Empty(extension.ValuesDescriptions);
        Assert.Empty(result);
    }
    [Fact]
    public void WritesEnumDescription()
    {
        // Arrange
        OpenApiEnumValuesDescriptionExtension extension = new();
        extension.EnumName = "TestEnum";
        extension.ValuesDescriptions = new()
        {
            new() {
                Description = "TestDescription",
                Value = "TestValue",
                Name = "TestName"
            }
        };
        using TextWriter sWriter = new StringWriter();
        OpenApiJsonWriter writer = new(sWriter);

        // Act
        extension.Write(writer, OpenApiSpecVersion.OpenApi3_0);
        string result = sWriter.ToString();

        // Assert
        Assert.Contains("values", result);
        Assert.Contains("modelAsString\": false", result);
        Assert.Contains("name\": \"TestEnum", result);
        Assert.Contains("description\": \"TestDescription", result);
        Assert.Contains("value\": \"TestValue", result);
        Assert.Contains("name\": \"TestName", result);
    }
}