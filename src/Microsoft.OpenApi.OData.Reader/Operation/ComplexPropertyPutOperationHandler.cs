// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyPutOperationHandler : ComplexPropertyUpdateOperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Put;
}
