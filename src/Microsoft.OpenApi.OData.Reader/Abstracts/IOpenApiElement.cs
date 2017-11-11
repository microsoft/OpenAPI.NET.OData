//---------------------------------------------------------------------
// <copyright file="IOpenApiElement.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Represents an Open API document element.
    /// </summary>
    internal interface IOpenApiDocumentGenerator
    {
        OpenApiDocument Generate();
    }
}
