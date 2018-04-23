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
    /// Operation segment
    /// </summary>
    public class ODataOperationSegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationSegment"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="unqualifiedCall">The unqualified call.</param>
        public ODataOperationSegment(IEdmOperation operation, bool unqualifiedCall)
        {
            Operation = operation ?? throw Error.ArgumentNull(nameof(operation));
            UnqualifiedCall = unqualifiedCall;
        }

        /// <summary>
        /// Gets the operation.
        /// </summary>
        public IEdmOperation Operation { get; }

        /// <summary>
        /// Gets the unqualified call.
        /// </summary>
        public bool UnqualifiedCall { get; }

        /// <inheritdoc />
        public override IEdmEntityType EntityType => throw new NotImplementedException();

        /// <inheritdoc />
        public override string ToString()
        {
            if (Operation.IsFunction())
            {
                return FunctionName(Operation as IEdmFunction);
            }

            return ActionName(Operation as IEdmAction);
        }

        private string FunctionName(IEdmFunction function)
        {
            StringBuilder functionName = new StringBuilder();
            if (UnqualifiedCall)
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

        private string ActionName(IEdmAction action)
        {
            if (UnqualifiedCall)
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