//---------------------------------------------------------------------
// <copyright file="OpenApiYamlWriterTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiYamlWriterTest : OpenApiWriterTest
    {
        private readonly ITestOutputHelper output;

        public OpenApiYamlWriterTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void WriteEmptyWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("", WriteEmptyAction.Write(OpenApiTarget.Yaml));
        }

        [Fact]
        public void WriteEmptyObjectWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("", WriteEmptyObjectAction.Write(OpenApiTarget.Yaml));
        }

        [Fact]
        public void WriteEmptyArrayWorks()
        {
            // Arrange & Act & Assert
            Assert.Equal("", WriteEmptyArrayAction.Write(OpenApiTarget.Yaml));
        }

        [Fact]
        public void WriteObjectWithPropertiesWorks()
        {
            // Act & Assert
            Assert.Equal("{\n  \"name\": \"value\"\n}", WriteObjectWithPropertiesAction.Write(OpenApiTarget.Yaml));
        }

        [Fact]
        public void WriteEmptyObjectWorks1()
        {
            // Arrange
            Action<OpenApiYamlWriter> writerAction = writer =>
            {
                writer.WriteStartObject();

                writer.WriteRequiredProperty("name", "API Support");

                writer.WriteRequiredProperty("url", "http://www.example.com/support");

                writer.WriteEndObject();
            };

            // Act & Assert
            //output.WriteLine(Write(writerAction));
            Assert.Equal(" ", Write(writerAction));
        }

        [Fact]
        public void WriteEmptyArrayWorks2()
        {
            // Arrange
            Action<OpenApiYamlWriter> writerAction = writer =>
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            };

            // Act & Assert
            Assert.Equal("[ ]", Write(writerAction));
        }

        [Fact]
        public void WriteObjectWithPropertiesWorks3()
        {
            // Arrange
            Action<OpenApiYamlWriter> writerAction = writer =>
            {
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("security");
                    {
                        writer.WriteStartArray();
                        {
                            writer.WriteStartObject();
                            {
                                writer.WritePropertyName("petstore_auth");
                                {
                                    writer.WriteStartArray();
                                    {
                                        writer.WriteValue("write:pets");
                                        writer.WriteValue("read:pets");
                                    }
                                    writer.WriteEndArray();
                                }


                                writer.WriteRequiredProperty("abc", "xxxxyyyy");

                                writer.WritePropertyName("petstore_website");
                                {
                                    writer.WriteStartArray();
                                    {
                                        writer.WriteValue("write:pets");
                                        writer.WriteValue("read:pets");
                                    }
                                    writer.WriteEndArray();
                                }
                            }
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                    }
                }
                writer.WriteEndObject();
            };

            string yaml = Write(writerAction);
            output.WriteLine(yaml);

            // Act & Assert
            Assert.Equal("{\n  \"name\": \"value\"\n}", yaml);
        }

        [Fact]
        public void WriteObjectWithPropertiesWorks2()
        {
            // Arrange
            Action<OpenApiYamlWriter> writerAction = writer =>
            {
                writer.WriteObject(() =>
                {
                    writer.WriteProperty("security", () =>
                    {
                        writer.WriteArray(() =>
                        {
                            writer.WriteObject(() =>
                            {
                                writer.WriteProperty("petstore_auth", () =>
                                {
                                    writer.WriteArray(() =>
                                    {
                                        writer.WriteValue("write:pets");
                                        writer.WriteValue("read:pets");
                                    });
                                });
                            });
                        });
                    });
                });
            };

            string yaml = Write(writerAction);
            output.WriteLine(yaml);

            // Act & Assert
            Assert.Equal("{\n  \"name\": \"value\"\n}", yaml);
        }

        private static string Write(Action<OpenApiYamlWriter> action)
        {
            MemoryStream stream = new MemoryStream();
            OpenApiYamlWriter writer = new OpenApiYamlWriter(new StreamWriter(stream));

            action(writer);

            writer.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
