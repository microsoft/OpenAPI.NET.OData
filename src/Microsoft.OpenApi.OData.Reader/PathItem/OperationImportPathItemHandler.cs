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
    /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmOperationImport"/>.
    /// </summary>
    internal class OperationImportPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.OperationImport;

        /// <summary>
        /// Gets the operation import.
        /// </summary>
        public IEdmOperationImport EdmOperationImport { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            if (EdmOperationImport.IsActionImport())
            {
                // Each action import is represented as a name/value pair whose name is the service-relative
                // resource path of the action import prepended with a forward slash, and whose value is a Path
                // Item Object containing the keyword post with an Operation Object as value that describes
                // how to invoke the action import.
                AddOperation(item, OperationType.Post);
            }
            else
            {
                // Each function import is represented as a name/value pair whose name is the service-relative
                // resource path of the function import prepended with a forward slash, and whose value is a Path
                // Item Object containing the keyword get with an Operation Object as value that describes
                // how to invoke the function import.
                AddOperation(item, OperationType.Get);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            ODataOperationImportSegment operationImportSegment = path.FirstSegment as ODataOperationImportSegment;
            EdmOperationImport = operationImportSegment.OperationImport;
        }
    }
}
