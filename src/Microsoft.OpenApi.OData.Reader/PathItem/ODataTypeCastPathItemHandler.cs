// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem;

/// <summary>
/// Path item handler for type cast for example: ~/groups/{id}/members/microsoft.graph.user
/// </summary>
internal class ODataTypeCastPathItemHandler : PathItemHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataTypeCastPathItemHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use for references lookup.</param>
    public ODataTypeCastPathItemHandler(OpenApiDocument document) : base(document)
    {
        
    }
    /// <inheritdoc/>
    protected override ODataPathKind HandleKind => ODataPathKind.TypeCast;

    /// <inheritdoc/>
    protected override void SetOperations(OpenApiPathItem item)
    {
        AddOperation(item, HttpMethod.Get);
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
    private IEdmStructuredType? StructuredType { get; set; }

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiPathItem pathItem)
    {
        base.SetBasicInfo(pathItem);
        if (StructuredType is IEdmNamedElement namedElement)
            pathItem.Description = $"Casts the previous resource to {namedElement.Name}.";
    }

    /// <inheritdoc/>
    protected override void SetExtensions(OpenApiPathItem item)
    {
        base.SetExtensions(item);
        if (StructuredType is null || Context is null) return;
        item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        item.Extensions.AddCustomAttributesToExtensions(Context, StructuredType);
    }
}
