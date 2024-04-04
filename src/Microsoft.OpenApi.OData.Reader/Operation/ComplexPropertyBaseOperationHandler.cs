// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Operation;

internal abstract class ComplexPropertyBaseOperationHandler : OperationHandler
{
    protected ODataComplexPropertySegment ComplexPropertySegment;
    
    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);
        ComplexPropertySegment = path.LastSegment as ODataComplexPropertySegment ?? throw Error.ArgumentNull(nameof(path));
    }

    /// <inheritdoc/>
    protected override void SetTags(OpenApiOperation operation)
    {
        string tagName = EdmModelHelper.GenerateComplexPropertyPathTagName(Path, Context);

        if (!string.IsNullOrEmpty(tagName))
        {
            OpenApiTag tag = new()
            {
                Name = tagName
            };

            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);
        }
        
        base.SetTags(operation);
    }
}