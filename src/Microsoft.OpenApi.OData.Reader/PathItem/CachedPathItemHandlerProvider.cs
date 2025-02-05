// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem;
/// <summary>
/// Cached path item handler provider.
/// </summary>
internal class CachedPathItemHandlerProvider : IPathItemHandlerProvider
{
    private readonly IPathItemHandlerProvider _concreteProvider;
	private readonly Dictionary<ODataPathKind, IPathItemHandler> _cache = new();
    public CachedPathItemHandlerProvider(IPathItemHandlerProvider concreteProvider)
	{
		Utils.CheckArgumentNull(concreteProvider, nameof(concreteProvider));
		_concreteProvider = concreteProvider;
	}
	/// <inheritdoc/>
	public IPathItemHandler GetHandler(ODataPathKind pathKind)
	{
		if (!_cache.TryGetValue(pathKind, out IPathItemHandler handler))
		{
			handler = _concreteProvider.GetHandler(pathKind);
			_cache[pathKind] = handler;
		}
		return handler;
	}
}