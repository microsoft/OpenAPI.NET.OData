// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Operation import segment.
    /// </summary>
    public class ODataOperationImportSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationImportSegment"/> class.
        /// </summary>
        /// <param name="operationImport">The operation import.</param>
        public ODataOperationImportSegment(IEdmOperationImport operationImport)
        {
            OperationImport = operationImport ?? throw Error.ArgumentNull(nameof(operationImport));
        }

        /// <summary>
        /// Gets the operation import.
        /// </summary>
        public IEdmOperationImport OperationImport { get; }

        /// <inheritdoc />
        public override IEdmEntityType EntityType => throw new System.NotImplementedException();

        /// <inheritdoc />
        public override string Name => OperationImport.Name;

        /// <inheritdoc />
        public override string ToString() => OperationImport.Name;
    }
}