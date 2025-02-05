

// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation;

/// <summary>
/// A class to provide the <see cref="IOperationHandler"/>.
/// </summary>
internal class CachedOperationHandlerProvider : IOperationHandlerProvider
{
    private readonly IOperationHandlerProvider _concreteProvider;
	private readonly Dictionary<(ODataPathKind, OperationType), IOperationHandler> _cache = new();
    public CachedOperationHandlerProvider(IOperationHandlerProvider concreteProvider)
	{
		Utils.CheckArgumentNull(concreteProvider, nameof(concreteProvider));
		_concreteProvider = concreteProvider;
	}
	/// <inheritdoc/>
	public IOperationHandler GetHandler(ODataPathKind pathKind, OperationType operationType)
	{
		if (!_cache.TryGetValue((pathKind, operationType), out IOperationHandler handler))
		{
			handler = _concreteProvider.GetHandler(pathKind, operationType);
			_cache[(pathKind, operationType)] = handler;
		}
		return handler;
	}
}