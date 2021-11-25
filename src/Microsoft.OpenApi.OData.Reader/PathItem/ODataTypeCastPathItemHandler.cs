// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem;

/// <summary>
/// Path item handler for type cast for example: ~/groups/{id}/members/microsoft.graph.user
/// </summary>
internal class ODataTypeCastPathItemHandler : PathItemHandler
{
	/// <inheritdoc/>
	protected override ODataPathKind HandleKind => ODataPathKind.TypeCast;

	/// <inheritdoc/>
	protected override void SetOperations(OpenApiPathItem item)
	{
		AddOperation(item, OperationType.Get);
	}
}
