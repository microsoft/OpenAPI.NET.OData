// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// The Open Api operation for <see cref="IEdmAction"/>.
    /// </summary>
    internal class EdmActionOperationHandler : EdmOperationOperationHandler
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EdmActionOperationHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use to lookup references.</param>
        public EdmActionOperationHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        public override HttpMethod OperationType => HttpMethod.Post;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            base.SetBasicInfo(operation);

            var insertRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            
            if (Context is not null && EdmOperation is not null)
            {
                var operationReadRestrictions = Context?.Model.GetRecord<InsertRestrictionsType>(EdmOperation, CapabilitiesConstants.InsertRestrictions);
                insertRestrictions?.MergePropertiesIfNull(operationReadRestrictions);
                insertRestrictions ??= operationReadRestrictions;
            }

            // Description
            if (!string.IsNullOrWhiteSpace(insertRestrictions?.LongDescription))
            {
                operation.Description = insertRestrictions.LongDescription;
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            if (EdmOperation is IEdmAction action && Context?.CreateRequestBody(action, _document) is OpenApiRequestBody requestBody)
            {               
                if (Context.Model.OperationTargetsMultiplePaths(action))
                {
                    operation.RequestBody = new OpenApiRequestBodyReference($"{action.Name}RequestBody", _document);
                }
                else
                {
                    operation.RequestBody = requestBody;
                }               
            }

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiAny("action"));
            base.SetExtensions(operation);
        }
    }
}
