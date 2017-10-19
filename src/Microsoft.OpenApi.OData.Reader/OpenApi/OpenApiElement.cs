//---------------------------------------------------------------------
// <copyright file="OpenApiElement.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    internal abstract class OpenApiElement : IOpenApiElement
    {
        public virtual void Write(IOpenApiWriter writer)
        {
            // nothing here
        }
    }
}
