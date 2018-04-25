// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create the bound operations for the navigation source.
    /// </summary>
    internal class OperationPathItemHandler : PathItemHandler
    {
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
            ODataOperationSegment operationSegment = path.LastSegment as ODataOperationSegment;
            EdmOperation = operationSegment.Operation;
            base.Initialize(context, path);
        }
    }
}
