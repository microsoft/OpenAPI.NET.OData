// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        /// Initializes a new instance of <see cref="ODataOperationImportSegment"/> class.
        /// </summary>
        /// <param name="operationImport">The operation import.</param>
        /// <param name="parameterMappings">The parameter mappings.</param>
        public ODataOperationImportSegment(IEdmOperationImport operationImport, IDictionary<string, string> parameterMappings)
        {
            OperationImport = operationImport ?? throw Error.ArgumentNull(nameof(operationImport));
            ParameterMappings = parameterMappings ?? throw Error.ArgumentNull(nameof(parameterMappings));
        }

        /// <summary>
        /// Gets the parameter mappings.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; }

        /// <summary>
        /// Gets the operation import.
        /// </summary>
        public IEdmOperationImport OperationImport { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.OperationImport;

        /// <inheritdoc />
        public override string Identifier { get => OperationImport.Name; }

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            if (OperationImport.IsFunctionImport())
            {
                return FunctionImportName(OperationImport as IEdmFunctionImport, settings, parameters);
            }

            return OperationImport.Name;
        }

        private string FunctionImportName(IEdmFunctionImport functionImport, OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            StringBuilder functionName = new StringBuilder(functionImport.Name);
            functionName.Append("(");

            // Structured or collection-valued parameters are represented as a parameter alias in the path template
            // and the parameters array contains a Parameter Object for the parameter alias as a query option of type string.
            IEdmFunction function = functionImport.Function;
            functionName.Append(String.Join(",", function.Parameters.Select(p =>
            {
                string uniqueName = Utils.GetUniqueName(p.Name, parameters);
                if (p.Type.IsStructured() || p.Type.IsCollection())
                {
                    return p.Name + "=@" + uniqueName;
                }
                else
                {
                    var quote = p.Type.Definition.ShouldPathParameterBeQuoted() ? "'" : string.Empty;
                    return $"{p.Name}={quote}{{{uniqueName}}}{quote}";
                }
            })));

            functionName.Append(")");
            return functionName.ToString();
        }
    }
}