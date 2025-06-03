// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create the bound operations for the navigation source.
    /// </summary>
    internal class OperationPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public OperationPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Operation;

        /// <summary>
        /// Gets the Edm operation.
        /// </summary>
        public IEdmOperation? EdmOperation { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            if (EdmOperation.IsAction())
            {
                // The Path Item Object for a bound action contains the keyword post,
                // The value of the operation keyword is an Operation Object that describes how to invoke the action.
                AddOperation(item, HttpMethod.Post);
            }
            else
            {
                // The Path Item Object for a bound function contains the keyword get,
                // The value of the operation keyword is an Operation Object that describes how to invoke the function.
                AddOperation(item, HttpMethod.Get);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (path.LastSegment is ODataOperationSegment {Operation: {} operation})
                EdmOperation = operation;
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            if (Context is null || !Context.Settings.ShowMsDosGroupPath)
            {
                return;
            }

            if (Path?.FirstSegment is ODataNavigationSourceSegment {NavigationSource: IEdmNavigationSource currentNavSource})
            {

                var samePaths = new List<ODataPath>();
                foreach (var path in Context.AllPaths.Where(p => p.Kind == ODataPathKind.Operation && p != Path))
                {
                    if (path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment && currentNavSource != navigationSourceSegment.NavigationSource)
                    {
                        continue;
                    }

                    if (path.LastSegment is ODataOperationSegment operationSegment && EdmOperation.FullName() != operationSegment.Operation.FullName())
                    {
                        continue;
                    }

                    samePaths.Add(path);
                }

                if (samePaths.Any())
                {
                    JsonArray array = new JsonArray();
                    OpenApiConvertSettings settings = Context.Settings.Clone();
                    settings.EnableKeyAsSegment = Context.KeyAsSegment;
                    foreach (var p in samePaths)
                    {
                        array.Add(p.GetPathItemName(settings));
                    }

                    item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                    item.Extensions.Add(Constants.xMsDosGroupPath, new JsonNodeExtension(array));
                }
            }

            base.SetExtensions(item);
            if (EdmOperation is not null)
            {
                item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                item.Extensions.AddCustomAttributesToExtensions(Context, EdmOperation);
            }
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to call the {EdmOperation?.Name} method.";
        }
    }
}
