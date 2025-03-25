// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for entity set operation.
    /// </summary>
    internal abstract class EntitySetOperationHandler : OperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EntitySetOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        protected EntitySetOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <summary>
        /// Gets/sets the <see cref="IEdmEntitySet"/>.
        /// </summary>
        protected IEdmEntitySet? EntitySet { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            // get the entity set.
            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: IEdmEntitySet navigationSource})
                EntitySet = navigationSource;
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            var tagName = EntitySet?.Name + "." + EntitySet?.EntityType.Name;
            operation.Tags ??= new HashSet<OpenApiTagReference>();
            operation.Tags.Add(new OpenApiTagReference(tagName, _document));

            Context?.AddExtensionToTag(tagName, Constants.xMsTocType, new OpenApiAny("page"), () => new OpenApiTag()
			{
				Name = tagName
			});

            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiAny("operation"));

            base.SetExtensions(operation);
        }

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context is {Settings.ShowExternalDocs: true} && CustomLinkRel is not null)
            {
                var externalDocs = (string.IsNullOrEmpty(TargetPath) ? null : Context.Model.GetLinkRecord(TargetPath, CustomLinkRel)) ??
                    (EntitySet is null ? null : Context.Model.GetLinkRecord(EntitySet, CustomLinkRel));

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
}
