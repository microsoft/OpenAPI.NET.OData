//---------------------------------------------------------------------
// <copyright file="OpenApiDocumentTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiDocumentTest
    {
        private readonly ITestOutputHelper output;

        public OpenApiDocumentTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CanWriterMemeoryStream()
        {
            OpenApiDocument doc = new OpenApiDocument();

            MemoryStream stream = new MemoryStream();
            doc.Write(stream);

            stream.Position = 0;
            string value = new StreamReader(stream).ReadToEnd();

            output.WriteLine(value);
        }

        [Fact]
        public void CanWriter()
        {
            OpenApiDocument doc = new OpenApiDocument();

            var builder = new StringBuilder();
            StringWriter sw = new StringWriter(builder);
            IOpenApiWriter writer = new OpenApiJsonWriter(sw, new OpenApiWriterSettings());
            doc.Write(writer);

            sw.Flush();

            output.WriteLine(sw.ToString());
        }

        [Fact]
        public void CanWriterYaml()
        {
            OpenApiDocument doc = new OpenApiDocument();

            var builder = new StringBuilder();
            StringWriter sw = new StringWriter(builder);
            IOpenApiWriter writer = new OpenApiYamlWriter(sw, new OpenApiWriterSettings());
            doc.Write(writer);

            sw.Flush();

            output.WriteLine(sw.ToString());
        }
    }
}
