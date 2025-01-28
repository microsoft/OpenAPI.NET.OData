// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
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
        AddReadOperation(item);
        AddUpdateOperation(item);
        AddInsertOperation(item);					
	}

    public void AddReadOperation(OpenApiPathItem item)
    {
        ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
        ReadRestrictionsType complexTypeReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(ComplexProperty, CapabilitiesConstants.ReadRestrictions);
        readRestrictions?.MergePropertiesIfNull(complexTypeReadRestrictions);
        readRestrictions ??= complexTypeReadRestrictions;
        bool isReadable = readRestrictions?.Readable ?? false;
        if ((Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths && isReadable) ||
            !Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths)
        {
            AddOperation(item, OperationType.Get);
        }
    }

	public void AddInsertOperation(OpenApiPathItem item)
	{
        if (Path.LastSegment is ODataComplexPropertySegment segment && segment.Property.Type.IsCollection())
        {
            InsertRestrictionsType insertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            InsertRestrictionsType entityInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(ComplexProperty, CapabilitiesConstants.InsertRestrictions);
            insertRestrictions?.MergePropertiesIfNull(entityInsertRestrictions);
            insertRestrictions ??= entityInsertRestrictions;
            bool isInsertable = insertRestrictions?.Insertable ?? false;
            if ((Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths && isInsertable) ||
                !Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths)
            {
                AddOperation(item, OperationType.Post);
            }
        }
    }

	public void AddUpdateOperation(OpenApiPathItem item)
	{
        UpdateRestrictionsType updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
        UpdateRestrictionsType complexTypeUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(ComplexProperty, CapabilitiesConstants.UpdateRestrictions);
        updateRestrictions?.MergePropertiesIfNull(complexTypeUpdateRestrictions);
        updateRestrictions ??= complexTypeUpdateRestrictions;
        bool isUpdatable = updateRestrictions?.Updatable ?? false;
        if ((Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths && isUpdatable) ||
            !Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths)
        {
            if (updateRestrictions?.IsUpdateMethodPutAndPatch == true)
            {
                AddOperation(item, OperationType.Put);
                AddOperation(item, OperationType.Patch);
            }
            else if (updateRestrictions?.IsUpdateMethodPut == true)
            {
                AddOperation(item, OperationType.Put);
            }
            else
            {
                AddOperation(item, OperationType.Patch);
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

	/// <inheritdoc/>
	protected override void SetExtensions(OpenApiPathItem item)
	{
		item.Extensions.AddCustomAttributesToExtensions(Context, ComplexProperty);
	}
}