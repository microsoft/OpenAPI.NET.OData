// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// The Open Api operation for <see cref="IEdmAction"/>.
    /// </summary>
    internal class EdmActionOperationHandler : EdmOperationOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        /// <summary>
        /// Gets the Edm Action.
        /// </summary>
        public IEdmAction Action => EdmOperation as IEdmAction;

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            IEdmAction action = EdmOperation as IEdmAction;
            if (action != null)
            {
                operation.RequestBody = Context.CreateRequestBody(action);
            }
        }
    }
}
