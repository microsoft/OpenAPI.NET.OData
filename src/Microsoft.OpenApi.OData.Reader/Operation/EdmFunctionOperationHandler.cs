// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// The Open Api operation for <see cref="IEdmFunction"/>.
    /// </summary>
    internal class EdmFunctionOperationHandler : EdmOperationOperationHandler
    {
        /// <inheritdoc/>
        public override OperationType OperationType => OperationType.Get;

        /// <summary>
        /// Gets the Edm Function.
        /// </summary>
        public IEdmFunction Function => EdmOperation as IEdmFunction;

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiOperation operation)
        {
            base.SetBasicInfo(operation);

            // Description
            var readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(EdmOperation, CapabilitiesConstants.ReadRestrictions);
            if (!string.IsNullOrWhiteSpace(readRestrictions?.LongDescription))
            {
                operation.Description = readRestrictions.LongDescription;
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiString("function"));
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

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context.Settings.ShowExternalDocs)
            {
                LinksType externalDocs = Context.Model.GetExternalDocs(Function, OperationType);
                if (externalDocs != null)
                {
                    operation.ExternalDocs = new OpenApiExternalDocs()
                    {
                        Description = CoreConstants.ExternalDocsDescription,
                        Url = externalDocs.Href
                    };
                }
            }
        }
    }
}
