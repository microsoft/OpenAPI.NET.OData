// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

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

                // so far, <Term Name="ReadRestrictions" Type="Capabilities.ReadRestrictionsType" AppliesTo="EntitySet Singleton FunctionImport">
                ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
                ReadRestrictionsType operationReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EdmOperationImport, CapabilitiesConstants.ReadRestrictions);
                readRestrictions?.MergePropertiesIfNull(operationReadRestrictions);
                readRestrictions ??= operationReadRestrictions;
                if (readRestrictions?.IsReadable ?? true)
                {
                    AddOperation(item, OperationType.Get);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            ODataOperationImportSegment operationImportSegment = path.FirstSegment as ODataOperationImportSegment;
            EdmOperationImport = operationImportSegment.OperationImport;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to call the {EdmOperationImport.Name} method.";
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            base.SetExtensions(item);
            item.Extensions.AddCustomAttributesToExtensions(Context, EdmOperationImport);
        }
    }
}
