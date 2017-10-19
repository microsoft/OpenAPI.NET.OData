//---------------------------------------------------------------------
// <copyright file="OpenApiCallback.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Callback Object: A map of possible out-of band callbacks related to the parent operation.
    /// The key value used to identify the callback object is an expression, evaluated at runtime, that identifies a URL to use for the callback operation.
    /// Each value in the map is a Path Item Object that describes a set of requests that may be initiated by the API provider and the expected responses.
    /// </summary>
    internal class OpenApiCallback : IOpenApiElement, IOpenApiWritable
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

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
