// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Net.Http;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyPatchOperationHandler : ComplexPropertyUpdateOperationHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComplexPropertyPatchOperationHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use to lookup references.</param>
    public ComplexPropertyPatchOperationHandler(OpenApiDocument document):base(document)
    {
        
    }
    /// <inheritdoc />
    public override HttpMethod OperationType => HttpMethod.Patch;
}
