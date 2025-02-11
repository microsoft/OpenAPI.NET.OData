// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Operation;

internal abstract class ComplexPropertyBaseOperationHandler : OperationHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComplexPropertyBaseOperationHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use to lookup references.</param>
    protected ComplexPropertyBaseOperationHandler(OpenApiDocument document) : base(document)
    {
        
    }
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
            Context.AddExtensionToTag(tagName, Constants.xMsTocType, new OpenApiAny("page"), () => new OpenApiTag()
			{
				Name = tagName
			});
            operation.Tags.Add(new OpenApiTagReference(tagName, _document));
        }
        
        base.SetTags(operation);
    }

    /// <inheritdoc/>
    protected override void SetExternalDocs(OpenApiOperation operation)
    {
        if (Context.Settings.ShowExternalDocs)
        {
            var externalDocs = Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) ??
                Context.Model.GetLinkRecord(ComplexPropertySegment.Property, CustomLinkRel);

            if (externalDocs != null)
            {
                operation.ExternalDocs = new OpenApiExternalDocs()
                {
                    Description = CoreConstants.ExternalDocsDescription,
                    Url = externalDocs.Href
                };
            }
        }
    }
}