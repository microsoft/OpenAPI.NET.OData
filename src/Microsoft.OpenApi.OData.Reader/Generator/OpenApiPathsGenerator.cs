﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiPaths"/> by Edm model.
    /// </summary>
    internal static class OpenApiPathsGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiPaths"/>
        /// The value of paths is a Paths Object.
        /// It is the main source of information on how to use the described API.
        /// It consists of name/value pairs whose name is a path template relative to the service root URL,
        /// and whose value is a Path Item Object.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="document">The Open API document to use to lookup references.</param>
        public static void AddPathsToDocument(this ODataContext context, OpenApiDocument document)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(document, nameof(document));

            // Due to the power and flexibility of OData a full representation of all service capabilities
            // in the Paths Object is typically not feasible, so this mapping only describes the minimum
            // information desired in the Paths Object.
            context.AddPathItemsToDocument(document);
        }
    }
}
