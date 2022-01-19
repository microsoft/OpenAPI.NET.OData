// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation;

internal abstract class ComplexPropertyBaseOperationHandler : OperationHandler
{
	/// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
    {
        ComplexPropertySegment = path.LastSegment as ODataComplexPropertySegment ?? throw Error.ArgumentNull(nameof(path));
    }
    protected ODataComplexPropertySegment ComplexPropertySegment;
}