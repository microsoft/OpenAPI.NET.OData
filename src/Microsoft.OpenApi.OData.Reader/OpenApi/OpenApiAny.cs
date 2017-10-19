//---------------------------------------------------------------------
// <copyright file="OpenApiAny.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Any object.
    /// TODO: it looks wrong
    /// </summary>
    internal class OpenApiAny : Dictionary<string, object>, IOpenApiElement, IOpenApiWritable
    {
        /// <summary>
        /// Write Any object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            foreach (var item in this)
            {
                writer.WritePropertyName(item.Key);

                IOpenApiWritable writerElement = item.Value as IOpenApiWritable;
                if (writerElement != null)
                {
                    writerElement.Write(writer);
                }
                else
                {
                    writer.WriteValue(item.Value);
                }
            }

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
