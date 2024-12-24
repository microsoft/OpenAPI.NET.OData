// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// The Open Api operation for <see cref="IEdmFunctionImport"/>.
    /// </summary>
    internal class EdmFunctionImportOperationHandler : EdmOperationImportOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EdmFunctionImportOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EdmFunctionImportOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            IEdmFunctionImport functionImport = EdmOperationImport as IEdmFunctionImport;

            if (OperationImportSegment.ParameterMappings != null)
            {
                foreach (var param in Context.CreateParameters(functionImport.Function, OperationImportSegment.ParameterMappings))
                {
                    operation.Parameters.AppendParameter(param);
                }
            }
            else
            {
                //The parameters array contains a Parameter Object for each parameter of the function overload,
                // and it contains specific Parameter Objects for the allowed system query options.
                foreach (var param in Context.CreateParameters(functionImport))
                {
                    operation.Parameters.AppendParameter(param);
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiAny("functionImport"));
            base.SetExtensions(operation);
        }
    }
}
