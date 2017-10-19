//---------------------------------------------------------------------
// <copyright file="OpenApiServerVariableTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiServerVariableTest
    {
        internal static OpenApiServerVariable BasicServerVariable = new OpenApiServerVariable();
        internal static OpenApiServerVariable AdvanceServerVariable = new OpenApiServerVariable
        {
            Default = "server variable default",
            Description = "server variable description string",
            Enums = new List<string>
            {
                "a",
                "b",
                "c"
            }
        };

        [Fact]
        public void WriteBasicServerVariableToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""default"": ""Default Default""
}".Replace();

            // Act & Assert
            Assert.Equal(expect, BasicServerVariable.WriteToJson());
        }

        [Fact]
        public void WriteBasicServerVariableToYamlWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("default: Default Default", BasicServerVariable.WriteToYaml());
        }

        [Fact]
        public void WriteAdvanceServerVariableToJsonWorks()
        {
            // Arrange
            string expect = @"
{
  ""default"": ""server variable default"",
  ""description"": ""server variable description string"",
  ""enum"": [
    ""a"",
    ""b"",
    ""c""
  ]
}"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceServerVariable.WriteToJson());
        }

        [Fact]
        public void WriteAdvanceServerVariableToYamlWorks()
        {
            // Arrange
            string expect = @"
default: server variable default
description: server variable description string
enum:
  - a
  - b
  - c
"
.Replace();

            // Act & Assert
            Assert.Equal(expect, AdvanceServerVariable.WriteToYaml());
        }
    }
}
