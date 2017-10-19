//---------------------------------------------------------------------
// <copyright file="OpenApiTarget.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Specifies what target of an Open API doc.
    /// </summary>
    public enum OpenApiTarget
    {
        /// <summary>
        /// The target is Open API version 3.0 JSON.
        /// </summary>
        Json,

        /// <summary>
        /// The target is Open API version 3.0 YAML.
        /// </summary>
        Yaml
    }
}
