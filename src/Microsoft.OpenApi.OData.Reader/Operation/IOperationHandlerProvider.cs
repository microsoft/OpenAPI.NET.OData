// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Net.Http;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// An interface to provider <see cref="IOperationHandler"/>.
    /// </summary>
    internal interface IOperationHandlerProvider
    {
        /// <summary>
        /// Get the <see cref="IOperationHandler"/>.
        /// </summary>
        /// <param name="pathKind">The path kind.</param>
        /// <param name="operationType">The operation type.</param>
        /// <param name="document">The Open API document to use to lookup references.</param>
        /// <returns>The corresponding <see cref="IOperationHandler"/>.</returns>
        IOperationHandler? GetHandler(ODataPathKind pathKind, HttpMethod operationType, OpenApiDocument document);
    }
}
