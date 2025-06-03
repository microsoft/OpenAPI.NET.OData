// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Net.Http;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// An interface to create a <see cref="OpenApiOperation"/> based on <see cref="ODataPath"/>.
    /// </summary>
    internal interface IOperationHandler
    {
        /// <summary>
        /// The operation type.
        /// </summary>
        HttpMethod OperationType { get; }

        /// <summary>
        /// Create the <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The OData path.</param>
        /// <returns>The created <see cref="OpenApiOperation"/>.</returns>
        OpenApiOperation CreateOperation(ODataContext context, ODataPath path);
    }
}
