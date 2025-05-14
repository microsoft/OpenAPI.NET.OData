// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
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
        public override HttpMethod OperationType => HttpMethod.Get;

        /// <inheritdoc/>
        protected override void SetParameters(OpenApiOperation operation)
        {
            base.SetParameters(operation);

            if (EdmOperationImport is not IEdmFunctionImport functionImport)
            {
                return;
            }

            if (OperationImportSegment?.ParameterMappings != null &&
                Context?.CreateParameters(functionImport.Function, _document, OperationImportSegment.ParameterMappings) is {} functionParamMappings)
            {
                operation.Parameters ??= [];
                foreach (var param in functionParamMappings)
                {
                    operation.Parameters.AppendParameter(param);
                }
            }
            else if (Context?.CreateParameters(functionImport, _document) is {} functionParams)
            {
                operation.Parameters ??= [];
                //The parameters array contains a Parameter Object for each parameter of the function overload,
                // and it contains specific Parameter Objects for the allowed system query options.
                foreach (var param in functionParams)
                {
                    operation.Parameters.AppendParameter(param);
                }
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions.Add(Constants.xMsDosOperationType, new JsonNodeExtension("functionImport"));
            base.SetExtensions(operation);
        }
    }
}
