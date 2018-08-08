// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Abstractions;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Authorizations
{
    /// <summary>
    /// The default 'Org.OData.Core.V1.Authorization' provider.
    /// </summary>
    internal class AuthorizationProvider
    {
        /// <summary>
        /// Gets the <see cref="IAuthorization"/> collections for a given target in the given Edm model.
        /// </summary>
        /// <returns>The <see cref="IAuthorization"/> collections.</returns>
        public virtual IEnumerable<Authorization> GetAuthorizations(IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            // Retrieve it every time when it needed. Don't want to cache the result.
            return RetrieveAuthorizations(model, target);
        }

        /// <summary>
        /// Create the corresponding Authorization object.
        /// </summary>
        /// <param name="record">The input record.</param>
        /// <returns>The created <see cref="Authorization"/> object.</returns>
        private IEnumerable<Authorization> RetrieveAuthorizations(IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            IEdmVocabularyAnnotation annotation = model.GetVocabularyAnnotation(target, AuthorizationConstants.Authorizations);
            if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.Collection)
            {
                IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;
                foreach (var item in collection.Elements)
                {
                    IEdmRecordExpression record = item as IEdmRecordExpression;
                    if (record == null || record.DeclaredType == null)
                    {
                        continue;
                    }

                    Authorization auth = Authorization.CreateAuthorization(record);
                    if (auth != null)
                    {
                        yield return auth;
                    }
                }
            }
        }
    }
}
