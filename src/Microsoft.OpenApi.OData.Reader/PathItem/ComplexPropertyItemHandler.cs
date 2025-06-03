// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem;

internal class ComplexPropertyItemHandler : PathItemHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComplexPropertyItemHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use for references lookup.</param>
    public ComplexPropertyItemHandler(OpenApiDocument document) : base(document)
    {
        
    }
    /// <inheritdoc/>
	protected override ODataPathKind HandleKind => ODataPathKind.ComplexProperty;

	/// <summary>
	/// Gets the complex property
	/// </summary>
	protected IEdmStructuralProperty? ComplexProperty { get; private set; }

	/// <inheritdoc/>
	protected override void SetOperations(OpenApiPathItem item)
	{
        AddReadOperation(item);
        AddUpdateOperation(item);
        AddInsertOperation(item);					
	}

    public void AddReadOperation(OpenApiPathItem item)
    {
        var readRestrictions = string.IsNullOrEmpty(TargetPath) ? null :  Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
        if (ComplexProperty is not null)
        {
            var complexTypeReadRestrictions = Context?.Model.GetRecord<ReadRestrictionsType>(ComplexProperty, CapabilitiesConstants.ReadRestrictions);
            readRestrictions?.MergePropertiesIfNull(complexTypeReadRestrictions);
            readRestrictions ??= complexTypeReadRestrictions;
        }
        bool isReadable = readRestrictions?.Readable ?? false;
        if (Context is not null && ((Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths && isReadable) ||
            !Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths))
        {
            AddOperation(item, HttpMethod.Get);
        }
    }

	public void AddInsertOperation(OpenApiPathItem item)
	{
        if (Path?.LastSegment is ODataComplexPropertySegment segment && segment.Property.Type.IsCollection())
        {
            var insertRestrictions = string.IsNullOrEmpty(TargetPath) ? null :  Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            if (ComplexProperty is not null)
            {
                var entityInsertRestrictions = Context?.Model.GetRecord<InsertRestrictionsType>(ComplexProperty, CapabilitiesConstants.InsertRestrictions);
                insertRestrictions?.MergePropertiesIfNull(entityInsertRestrictions);
                insertRestrictions ??= entityInsertRestrictions;
            }
            bool isInsertable = insertRestrictions?.Insertable ?? false;
            if (Context is not null && ((Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths && isInsertable) ||
                !Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths))
            {
                AddOperation(item, HttpMethod.Post);
            }
        }
    }

	public void AddUpdateOperation(OpenApiPathItem item)
	{
        var updateRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
        if (ComplexProperty is not null)
        {
            var complexTypeUpdateRestrictions = Context?.Model.GetRecord<UpdateRestrictionsType>(ComplexProperty, CapabilitiesConstants.UpdateRestrictions);
            updateRestrictions?.MergePropertiesIfNull(complexTypeUpdateRestrictions);
            updateRestrictions ??= complexTypeUpdateRestrictions;
        }
        bool isUpdatable = updateRestrictions?.Updatable ?? false;
        if (Context is not null && ((Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths && isUpdatable) ||
            !Context.Settings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths))
        {
            if (updateRestrictions?.IsUpdateMethodPutAndPatch == true)
            {
                AddOperation(item, HttpMethod.Put);
                AddOperation(item, HttpMethod.Patch);
            }
            else if (updateRestrictions?.IsUpdateMethodPut == true)
            {
                AddOperation(item, HttpMethod.Put);
            }
            else
            {
                AddOperation(item, HttpMethod.Patch);
            }
        }
    }

    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
	{
		base.Initialize(context, path);

		// The last segment should be the complex property segment.
		if (path.LastSegment is ODataComplexPropertySegment {Property: {} property})
		    ComplexProperty = property;
	}

	/// <inheritdoc/>
	protected override void SetExtensions(OpenApiPathItem item)
	{
        if (ComplexProperty is null) return;

        if (Context is not null)
        {
            item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
		    item.Extensions.AddCustomAttributesToExtensions(Context, ComplexProperty);
        }
	}
}