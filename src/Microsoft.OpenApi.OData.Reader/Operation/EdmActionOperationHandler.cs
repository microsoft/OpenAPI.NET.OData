// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Post;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            base.SetBasicInfo(operation);

            // Description
            var insertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(EdmOperation, CapabilitiesConstants.InsertRestrictions);
            if (!string.IsNullOrWhiteSpace(insertRestrictions?.LongDescription))
            {
                operation.Description = insertRestrictions.LongDescription;
            }
        }

        /// <inheritdoc/>
        protected override void SetRequestBody(OpenApiOperation operation)
        {
            if (EdmOperation is IEdmAction action)
            {
                if (Context.Model.OperationTargetsMultiplePaths(action))
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        UnresolvedReference = true,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.RequestBody,
                            Id = $"{action.Name}RequestBody"
                        }
                    };
                }
                else
                {
                    operation.RequestBody = Context.CreateRequestBody(action);
                }
            }

            base.SetRequestBody(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiString("action"));
            if (Context.Settings.EnablePagination && EdmOperation.ReturnType?.TypeKind() == EdmTypeKind.Collection)
            {
                OpenApiObject extension = new OpenApiObject
                {
                    { "nextLinkName", new OpenApiString("@odata.nextLink")},
                    { "operationName", new OpenApiString(Context.Settings.PageableOperationName)}
                };

                operation.Extensions.Add(Constants.xMsPageable, extension);              
            }
            base.SetExtensions(operation);
        }
    }
}
