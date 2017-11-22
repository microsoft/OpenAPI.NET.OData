// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.IO;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Writers;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    internal static class OpenApiWriterTestHelper
    {
        internal static string WriteToJson(this IOpenApiElement element,
            Action<IOpenApiWriter, IOpenApiElement> before = null,
            Action<IOpenApiWriter, IOpenApiElement> after = null)
        {
            Action<IOpenApiWriter> action = writer =>
            {
                before?.Invoke(writer, element);
                // element?(writer);
                after?.Invoke(writer, element);
                writer?.Flush();
            };

            return Write(OpenApiFormat.Json, action);
        }

        internal static string WriteToYaml(this IOpenApiElement element,
            Action<IOpenApiWriter, IOpenApiElement> before = null,
            Action<IOpenApiWriter, IOpenApiElement> after = null)
        {
            Action<IOpenApiWriter> action = writer =>
            {
                before?.Invoke(writer, element);
                //element?.Write(writer);
                after?.Invoke(writer, element);
                writer?.Flush();
            };

            return Write(OpenApiFormat.Yaml, action);
        }

        internal static string Write(this IOpenApiElement element,
            OpenApiFormat target,
            Action<IOpenApiWriter, IOpenApiElement> before = null,
            Action<IOpenApiWriter, IOpenApiElement> after = null)
        {
            Action<IOpenApiWriter> action = writer =>
            {
                before?.Invoke(writer, element);
               // element?.Write(writer);
                after?.Invoke(writer, element);
                writer?.Flush();
            };

            return Write(target, action);
        }

        internal static string Write(this Action<IOpenApiWriter> action, OpenApiFormat target)
        {
            MemoryStream stream = new MemoryStream();
            IOpenApiWriter writer;
            if (target == OpenApiFormat.Yaml)
            {
                writer = new OpenApiYamlWriter(new StreamWriter(stream));
            }
            else
            {
                writer = new OpenApiJsonWriter(new StreamWriter(stream));
            }

            action(writer);
            writer.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        internal static string Write(OpenApiFormat target, Action<IOpenApiWriter> action)
        {
            MemoryStream stream = new MemoryStream();
            IOpenApiWriter writer;
            if (target == OpenApiFormat.Yaml)
            {
                writer = new OpenApiYamlWriter(new StreamWriter(stream));
            }
            else
            {
                writer = new OpenApiJsonWriter(new StreamWriter(stream));
            }

            action(writer);
            writer.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        internal static string SerializeAsJson<T>(this T element)
            where T : IOpenApiSerializable
        {
            return element.Serialize(OpenApiFormat.Json);
        }

        internal static string SerializeAsYaml<T>(this T element)
            where T : IOpenApiSerializable
        {
            return element.Serialize(OpenApiFormat.Yaml);
        }

        internal static string Serialize<T>(this T element, OpenApiFormat format)
            where T : IOpenApiSerializable
        {
            MemoryStream stream = new MemoryStream();
            element.Serialize(stream, OpenApiSpecVersion.OpenApi3_0_0, format);
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
