//---------------------------------------------------------------------
// <copyright file="IOpenApiExtensible.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Represents an Extensible Open API element.
    /// </summary>
    internal interface IOpenApiExtensible : IOpenApiElement
    {
        /// <summary>
        /// Specification extensions.
        /// </summary>
        IList<OpenApiExtension> Extensions { get; }
    }
}
