//---------------------------------------------------------------------
// <copyright file="IOpenApiWritable.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Represents an Open API element is writable.
    /// </summary>
    internal interface IOpenApiWritable : IOpenApiElement
    {
        /// <summary>
        /// Write Open API element.
        /// </summary>
        /// <param name="writer">The writer.</param>
        void Write(IOpenApiWriter writer);
    }
}
