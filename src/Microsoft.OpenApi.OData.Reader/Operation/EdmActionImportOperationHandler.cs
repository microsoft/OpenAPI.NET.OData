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
    /// The Open Api operation for <see cref="IEdmActionImport"/>.
    /// </summary>
    internal class EdmActionImportOperationHandler : EdmOperationImportOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        protected override void SetRequestBody(OpenApiOperation operation)
        {
            IEdmActionImport actionImport = EdmOperationImport as IEdmActionImport;

            // The requestBody field contains a Request Body Object describing the structure of the request body.
            // Its schema value follows the rules for Schema Objects for complex types, with one property per action parameter.
            operation.RequestBody = Context.CreateRequestBody(actionImport);
        }
    }
}
