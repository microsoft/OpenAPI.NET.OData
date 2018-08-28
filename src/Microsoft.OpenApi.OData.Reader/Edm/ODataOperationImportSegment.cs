// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Text;
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
        public override ODataSegmentKind Kind => ODataSegmentKind.OperationImport;

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            if (OperationImport.IsFunctionImport())
            {
                return FunctionImportName(OperationImport as IEdmFunctionImport, settings);
            }

            return OperationImport.Name;
        }

        private string FunctionImportName(IEdmFunctionImport functionImport, OpenApiConvertSettings settings)
        {
            StringBuilder functionName = new StringBuilder(functionImport.Name);
            functionName.Append("(");

            // Structured or collection-valued parameters are represented as a parameter alias in the path template
            // and the parameters array contains a Parameter Object for the parameter alias as a query option of type string.
            IEdmFunction function = functionImport.Function;
            functionName.Append(String.Join(",", function.Parameters.Select(p =>
            {
                if (p.Type.IsStructured() || p.Type.IsCollection())
                {
                    return p.Name + "=@" + p.Name;
                }
                else
                {
                    return p.Name + "={" + p.Name + "}";
                }
            })));

            functionName.Append(")");
            return functionName.ToString();
        }
    }
}