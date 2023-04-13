// -----
// private readonly IEdmModel _model;-------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
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
        /// <param name="isEscapedFunction">A value indicating this operation is an escaped function.</param>
        public ODataOperationSegment(IEdmOperation operation, bool isEscapedFunction)
        {
            Operation = operation ?? throw Error.ArgumentNull(nameof(operation));
            IsEscapedFunction = isEscapedFunction;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationSegment"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="parameterMappings">The parameter mapping.</param>
        public ODataOperationSegment(IEdmOperation operation, IDictionary<string, string> parameterMappings)
        {
            Operation = operation ?? throw Error.ArgumentNull(nameof(operation));
            ParameterMappings = parameterMappings ?? throw Error.ArgumentNull(nameof(parameterMappings));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationSegment"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="model">The Edm model.</param>
        public ODataOperationSegment(IEdmOperation operation, IEdmModel model)
            : this(operation, false, model)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataOperationSegment"/> class.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="isEscapedFunction">A value indicating this operation is an escaped function.</param>
        /// <param name="model">The Edm model.</param>
        public ODataOperationSegment(IEdmOperation operation, bool isEscapedFunction, IEdmModel model)
        {
            Operation = operation ?? throw Error.ArgumentNull(nameof(operation));
            IsEscapedFunction = isEscapedFunction;
            _model = model ?? throw Error.ArgumentNull(nameof(model));
        }
        
        private readonly IEdmModel _model;
        
        /// <summary>
        /// Gets the parameter mappings.
        /// </summary>
        public IDictionary<string, string> ParameterMappings { get; }

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
        public override IEdmEntityType EntityType => null;

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            if (Operation.IsFunction())
            {
                return FunctionName(Operation as IEdmFunction, settings, parameters);
            }

            return OperationName(Operation, settings);
        }

        internal IDictionary<string, string> GetNameMapping(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            IDictionary<string, string> parameterNamesMapping = new Dictionary<string, string>();

            if (Operation.IsFunction())
            {
                IEdmFunction function = Operation as IEdmFunction;
                if (settings.EnableUriEscapeFunctionCall && IsEscapedFunction)
                {
                    string parameterName = function.Parameters.Last().Name;
                    string uniqueName = Utils.GetUniqueName(parameterName, parameters);
                    parameterNamesMapping[parameterName] = uniqueName;
                }

                int skip = function.IsBound ? 1 : 0;
                foreach (var parameter in function.Parameters.Skip(skip))
                {
                    string uniqueName = Utils.GetUniqueName(parameter.Name, parameters);
                    parameterNamesMapping[parameter.Name] = uniqueName;
                }
            }

            return parameterNamesMapping;
        }

        private string OperationName(IEdmOperation operation, OpenApiConvertSettings settings)
        {
            if (settings.EnableUnqualifiedCall)
            {
                return operation.Name;
            }
            else if (_model != null)
            {
                return EdmModelHelper.StripOrAliasNamespacePrefix(operation, settings, _model);
            }
            else
            {
                return operation.FullName();
            }
        }

        private string FunctionName(IEdmFunction function, OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            if (settings.EnableUriEscapeFunctionCall && IsEscapedFunction)
            {
                // Debug.Assert(function.Parameters.Count == 2); It should be verify at Edm model.
                // Debug.Assert(function.IsBound == true);
                string parameterName = function.Parameters.Last().Name;
                string uniqueName = Utils.GetUniqueName(parameterName, parameters);
                if (function.IsComposable)
                {
                    return $"{{{uniqueName}}}:";
                }
                else
                {
                    return $"{{{uniqueName}}}";
                }
            }

            StringBuilder functionName = new();
            functionName.Append(OperationName(function, settings));
            functionName.Append("(");
            
            int skip = function.IsBound ? 1 : 0;
            functionName.Append(string.Join(",", function.Parameters.Skip(skip).Select(p =>
            {
                string uniqueName = Utils.GetUniqueName(p.Name, parameters);
                var quote = p.Type.Definition.ShouldPathParameterBeQuoted(settings) ? "'" : string.Empty;
                return p is IEdmOptionalParameter
                    ? p.Name + $"={quote}@{uniqueName}{quote}"
                    : p.Name + $"={quote}{{{uniqueName}}}{quote}";
            })));

            functionName.Append(")");

            return functionName.ToString();
        }

        /// <inheritdoc />
		public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
		{
			return new IEdmVocabularyAnnotatable[] { Operation };
		}
    }
}