//---------------------------------------------------------------------
// <copyright file="OpenApiDictionaryOfT.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Open Api Dictionary of T.
    /// </summary>
    internal abstract class OpenApiDictionary<T> : Dictionary<string, T>,
        IOpenApiElement,
        IOpenApiWritable
        where T : IOpenApiElement, IOpenApiWritable
    {
        /// <summary>
        /// Add a key/value item.
        /// </summary>
        /// <param name="item">The key/value item.</param>
        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Add a Tuple(key, value) item.
        /// </summary>
        /// <param name="item">The Tuple(key, value) item.</param>
        public void Add(Tuple<string, T> item)
        {
            Add(item.Item1, item.Item2);
        }

        /// <summary>
        /// Write Any object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            // write something after start
            WriteAfterStart(writer);

            // path items
            foreach (var item in this)
            {
                writer.WriteRequiredObject(item.Key, item.Value);
            }

            // write something before end.
            WriteBeforeEnd(writer);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }

        public abstract void WriteAfterStart(IOpenApiWriter writer);

        public abstract void WriteBeforeEnd(IOpenApiWriter writer);
    }
}
