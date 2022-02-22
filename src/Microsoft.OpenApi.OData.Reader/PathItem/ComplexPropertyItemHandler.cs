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
		ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(ComplexProperty, CapabilitiesConstants.ReadRestrictions);
		bool isReadable = read?.IsReadable ?? false;
		if ((Context.Settings.UseRestrictionAnnotationsToGeneratePathsForComplexProperties && isReadable) ||
			Context.Settings.UseRestrictionAnnotationsToGeneratePathsForComplexProperties == false)
        {
			AddOperation(item, OperationType.Get);
		}		

		UpdateRestrictionsType update = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexProperty, CapabilitiesConstants.UpdateRestrictions);
		bool isUpdatable = update?.IsUpdatable ?? false;
		if ((Context.Settings.UseRestrictionAnnotationsToGeneratePathsForComplexProperties && isUpdatable) ||
			Context.Settings.UseRestrictionAnnotationsToGeneratePathsForComplexProperties == false) 
		{
			if (update != null && update.IsUpdateMethodPut)
			{
				AddOperation(item, OperationType.Put);
			}
			else
            {
				AddOperation(item, OperationType.Patch);
			}
		}
		
		InsertRestrictionsType insert = Context.Model.GetRecord<InsertRestrictionsType>(ComplexProperty, CapabilitiesConstants.InsertRestrictions);
		bool isInsertable = insert?.IsInsertable ?? false;
		if ((Context.Settings.UseRestrictionAnnotationsToGeneratePathsForComplexProperties && isInsertable) ||
			Context.Settings.UseRestrictionAnnotationsToGeneratePathsForComplexProperties == false)
        {
            if (Path.LastSegment is ODataComplexPropertySegment segment && segment.Property.Type.IsCollection())
            {
				AddOperation(item, OperationType.Post);
			}                
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