//---------------------------------------------------------------------
// <copyright file="OpenApiWriterTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class OpenApiWriterTest
    {
        internal Action<IOpenApiWriter> WriteEmptyAction { get; } = w => { };

        internal Action<IOpenApiWriter> WriteEmptyObjectAction { get; } = w =>
        {
            w.WriteStartObject();
            w.WriteEndObject();
        };

        internal Action<IOpenApiWriter> WriteEmptyArrayAction { get; } = w =>
        {
            w.WriteStartArray();
            w.WriteEndArray();
        };

        internal Action<IOpenApiWriter> WriteObjectWithPropertiesAction { get; } = w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("name");
            w.WriteValue("value");
            w.WriteEndObject();
        };
    }
}
