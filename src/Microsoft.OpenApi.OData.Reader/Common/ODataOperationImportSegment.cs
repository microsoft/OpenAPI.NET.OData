// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Operation import segment
    /// </summary>
    public class ODataOperationImportSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationImportSegment"/> class.
        /// </summary>
        /// <param name="operationImport"></param>
        public ODataOperationImportSegment(IEdmOperationImport operationImport)
        {
            OperationImport = operationImport ?? throw Error.ArgumentNull(nameof(operationImport));
        }

        public IEdmOperationImport OperationImport { get; }

        public override IEdmEntityType EntityType => throw new System.NotImplementedException();

        public override string ToString()
        {
            return OperationImport.Name;
        }
    }
}