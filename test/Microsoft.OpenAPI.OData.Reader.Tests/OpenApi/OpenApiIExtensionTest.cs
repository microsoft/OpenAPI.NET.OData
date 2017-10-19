//---------------------------------------------------------------------
// <copyright file="OpenApiExtensionTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.OData.OpenAPI.Properties;
using Xunit;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiExtensionTest
    {
        [Fact]
        public void CtorThrowsArgumentNullName()
        {
            Assert.Throws<ArgumentException>("name", () => new OpenApiExtension(null, null));
        }

        [Fact]
        public void CtorThrowsFieldNameMustPrefix()
        {
            // Arrange
            var exception = Assert.Throws<OpenApiException>(() => new OpenApiExtension("any", null));

            // Act & Assert
            Assert.Equal(SRResource.ExtensionFieldNameMustBeginWithXMinus, exception.Message);
        }

        [Fact]
        public void WriteExtensionWithNullValueWorks()
        {
            // Arrange
            OpenApiExtension ex = new OpenApiExtension("x-name", null);
            Action<IOpenApiWriter> action = writer =>
            {
                writer.WriteStartObject(); // for valid JSON/Yaml object
                {
                    ex.Write(writer);
                }

                writer.WriteEndObject();
                writer.Flush();
            };

            // Act & Assert
            Assert.Equal("{\n  \"x-name\": null\n}",
                OpenApiWriterTestHelper.Write(OpenApiTarget.Json, action));
        }
    }
}
