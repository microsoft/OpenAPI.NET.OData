// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
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
    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);
        if(path.LastSegment is ODataTypeCastSegment castSegment)
        {
            StructuredType = castSegment.StructuredType;
        }
    }
    private IEdmStructuredType StructuredType { get; set; }

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiPathItem pathItem)
    {
        base.SetBasicInfo(pathItem);
        pathItem.Description = $"Casts the previous resource to {(StructuredType as IEdmNamedElement).Name}.";
    }

    /// <inheritdoc/>
    protected override void SetExtensions(OpenApiPathItem pathItem)
    {
        base.SetExtensions(pathItem);
        pathItem.Extensions.AddCustomAttributesToExtensions(Context, StructuredType);
    }
}
