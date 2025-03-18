// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of <see cref="IEdmSingleton"/>.
    /// </summary>
    internal abstract class SingletonOperationHandler : OperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SingletonOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        protected SingletonOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        /// <summary>
        /// Gets the <see cref="IEdmSingleton"/>.
        /// </summary>
        protected IEdmSingleton Singleton { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            // Base Initialize should be called at top of this method.
            base.Initialize(context, path);

            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;

            Singleton = navigationSourceSegment.NavigationSource as IEdmSingleton;
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            // In this SDK, we use "[Singleton Name].[Singleton Entity Type Name]
            // For example: "Me.User"
            var tagName = Singleton.Name + "." + Singleton.EntityType.Name;

            Context.AddExtensionToTag(tagName, Constants.xMsTocType, new OpenApiAny("page"), () => new OpenApiTag()
			{
				Name = tagName
			});
            operation.Tags ??= new HashSet<OpenApiTagReference>();
            operation.Tags.Add(new OpenApiTagReference(tagName, _document));

            // Call base.SetTags() at the end of this method.
            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiAny("operation"));

            base.SetExtensions(operation);
        }

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context.Settings.ShowExternalDocs)
            {
                var externalDocs = Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) ??
                    Context.Model.GetLinkRecord(Singleton, CustomLinkRel);

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
