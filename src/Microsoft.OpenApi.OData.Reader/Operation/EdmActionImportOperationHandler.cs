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
    /// The Open Api operation for <see cref="IEdmActionImport"/>.
    /// </summary>
    internal class EdmActionImportOperationHandler : EdmOperationImportOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EdmActionImportOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EdmActionImportOperationHandler(OpenApiDocument document):base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Post;

        protected override void SetRequestBody(OpenApiOperation operation)
        {
            // The requestBody field contains a Request Body Object describing the structure of the request body.
            // Its schema value follows the rules for Schema Objects for complex types, with one property per action parameter.
            if (EdmOperationImport is IEdmActionImport actionImport)
                operation.RequestBody = Context?.CreateRequestBody(actionImport, _document);

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions.Add(Constants.xMsDosOperationType, new JsonNodeExtension("actionImport"));

            base.SetExtensions(operation);
        }
    }
}
