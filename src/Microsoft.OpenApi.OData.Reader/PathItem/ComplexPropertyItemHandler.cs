// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem;

internal class ComplexPropertyItemHandler : PathItemHandler
{
    /// <inheritdoc/>
	protected override ODataPathKind HandleKind => ODataPathKind.ComplexProperty;

	/// <summary>
	/// Gets the complex property
	/// </summary>
	protected IEdmStructuralProperty ComplexProperty { get; private set; }

	/// <inheritdoc/>
	protected override void SetOperations(OpenApiPathItem item)
	{
		AddOperation(item, OperationType.Get);

		UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexProperty);
		if (update != null && update.IsUpdateMethodPut)
		{
			AddOperation(item, OperationType.Put);
		}
		else
		{
			AddOperation(item, OperationType.Patch);
		}

		if(Path.LastSegment is ODataComplexPropertySegment segment)
		{
			if(segment.Property.Type.IsNullable)
				AddOperation(item, OperationType.Delete);
			if(segment.Property.Type.IsCollection())
				AddOperation(item, OperationType.Post);
		}
	}

	/// <inheritdoc/>
	protected override void Initialize(ODataContext context, ODataPath path)
	{
		base.Initialize(context, path);

		// The last segment should be the complex property segment.
		ODataComplexPropertySegment navigationSourceSegment = path.LastSegment as ODataComplexPropertySegment;
		ComplexProperty = navigationSourceSegment.Property;
	}
}