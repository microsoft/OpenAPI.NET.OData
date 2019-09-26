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
    /// Operation segment
    /// </summary>
    public class ODataOperationSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationSegment"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        public ODataOperationSegment(IEdmOperation operation)
            : this(operation, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationSegment"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        public ODataOperationSegment(IEdmOperation operation, bool isEscapedFunction)
        {
            Operation = operation ?? throw Error.ArgumentNull(nameof(operation));
            IsEscapedFunction = isEscapedFunction;
        }

        /// <summary>
        /// Gets the operation.
        /// </summary>
        public IEdmOperation Operation { get; }

        /// <summary>
        /// Gets the is escaped function.
        /// </summary>
        public bool IsEscapedFunction { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Operation;

        /// <inheritdoc />
        public override string Identifier { get => Operation.Name; }

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            if (Operation.IsFunction())
            {
                return FunctionName(Operation as IEdmFunction, settings, parameters);
            }

            return ActionName(Operation as IEdmAction, settings);
        }

        private string FunctionName(IEdmFunction function, OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            if (settings.EnableUriEscapeFunctionCall && IsEscapedFunction)
            {
                // Debug.Assert(function.Parameters.Count == 2); It should be verify at Edm model.
                // Debug.Assert(function.IsBound == true);
                string parameterName = function.Parameters.Last().Name;
                parameterName = Utils.GetUniqueName(parameterName, parameters);
                if (function.IsComposable)
                {
                    return $"{{{parameterName}}}:";
                }
                else
                {
                    return $"{{{parameterName}}}";
                }
            }

            StringBuilder functionName = new StringBuilder();
            if (settings.EnableUnqualifiedCall)
            {
                functionName.Append(function.Name);
            }
            else
            {
                functionName.Append(function.FullName());
            }
            functionName.Append("(");

            // Structured or collection-valued parameters are represented as a parameter alias in the path template
            // and the parameters array contains a Parameter Object for the parameter alias as a query option of type string.
            int skip = function.IsBound ? 1 : 0;
            functionName.Append(String.Join(",", function.Parameters.Skip(skip).Select(p =>
            {
                string uniqueName = Utils.GetUniqueName(p.Name, parameters);
                if (p.Type.IsStructured() || p.Type.IsCollection())
                {
                    return p.Name + "=@" + uniqueName;
                }
                else
                {
                    return p.Name + "={" + uniqueName + "}";
                }
            })));

            functionName.Append(")");

            return functionName.ToString();
        }

        private string ActionName(IEdmAction action, OpenApiConvertSettings settings)
        {
            if (settings.EnableUnqualifiedCall)
            {
                return action.Name;
            }
            else
            {
                return action.FullName();
            }
        }
    }
}