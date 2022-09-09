// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create the bound operations for the navigation source.
    /// </summary>
    internal class OperationPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Operation;

        /// <summary>
        /// Gets the Edm operation.
        /// </summary>
        public IEdmOperation EdmOperation { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            if (EdmOperation.IsAction())
            {
                // The Path Item Object for a bound action contains the keyword post,
                // The value of the operation keyword is an Operation Object that describes how to invoke the action.
                AddOperation(item, OperationType.Post);
            }
            else
            {
                // The Path Item Object for a bound function contains the keyword get,
                // The value of the operation keyword is an Operation Object that describes how to invoke the function.
                AddOperation(item, OperationType.Get);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            ODataOperationSegment operationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = operationSegment.Operation;
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            if (!Context.Settings.ShowMsDosGroupPath)
            {
                return;
            }

            ODataNavigationSourceSegment navigationSourceSegment = Path.FirstSegment as ODataNavigationSourceSegment;
            IEdmNavigationSource currentNavSource = navigationSourceSegment.NavigationSource;

            IList<ODataPath> samePaths = new List<ODataPath>();
            foreach (var path in Context.AllPaths.Where(p => p.Kind == ODataPathKind.Operation && p != Path))
            {
                navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
                if (currentNavSource != navigationSourceSegment.NavigationSource)
                {
                    continue;
                }

                ODataOperationSegment operationSegment = path.LastSegment as ODataOperationSegment;
                if (EdmOperation.FullName() != operationSegment.Operation.FullName())
                {
                    continue;
                }

                samePaths.Add(path);
            }

            if (samePaths.Any())
            {
                OpenApiArray array = new OpenApiArray();
                OpenApiConvertSettings settings = Context.Settings.Clone();
                settings.EnableKeyAsSegment = Context.KeyAsSegment;
                foreach (var p in samePaths)
                {
                    array.Add(new OpenApiString(p.GetPathItemName(settings)));
                }

                item.Extensions.Add(Constants.xMsDosGroupPath, array);
            }

            base.SetExtensions(item);
            item.Extensions.AddCustomAttributesToExtensions(Context, EdmOperation);            
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to call the {EdmOperation.Name} method.";
        }
    }
}
