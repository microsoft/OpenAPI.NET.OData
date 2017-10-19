//---------------------------------------------------------------------
// <copyright file="OpenApiDictionaryOfT.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Open Api Dictionary with extended of T.
    /// </summary>
    internal abstract class OpenApiDictionaryExtensibleOfT<T> : OpenApiDictionary<T>, IOpenApiExtensible
        where T : IOpenApiElement, IOpenApiWritable
    {
        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Write something before.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void WriteAfterStart(IOpenApiWriter writer)
        {
        }

        /// <summary>
        /// Write something after.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void WriteBeforeEnd(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // specification extensions
            writer.WriteDictionary(Extensions);
        }
    }
}
