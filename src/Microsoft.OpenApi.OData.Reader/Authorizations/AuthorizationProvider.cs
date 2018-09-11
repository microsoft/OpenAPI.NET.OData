// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
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
        /// Annotatable: EntitySet Singleton ActionImport FunctionImport Action Function
        /// Collection(Core.HttpRequest)
        /// </summary>
        private IDictionary<IEdmVocabularyAnnotatable, IEnumerable<Authorization>> _authorizations
            = new Dictionary<IEdmVocabularyAnnotatable, IEnumerable<Authorization>>();

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the Edm Term.
        /// </summary>
        public IEdmTerm Term { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationProvider"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public AuthorizationProvider(IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            Term = model.FindTerm(AuthorizationConstants.Authorizations);

            Model = model;
        }

        /// <summary>
        /// Gets the <see cref="Authorization"/> collections for a given target in the given Edm model.
        /// </summary>
        /// <param name="target">The Edm target.</param>
        /// <returns>The <see cref="Authorization"/> collections.</returns>
        public virtual IEnumerable<Authorization> GetAuthorizations(IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(target, nameof(target));

            if (_authorizations.TryGetValue(target, out IEnumerable<Authorization> value))
            {
                return value;
            }

            if (Term == null)
            {
                return Enumerable.Empty<Authorization>();
            }

            value = RetrieveAuthorizations(target);
            _authorizations[target] = value;
            return value;
        }

        /// <summary>
        /// Create the corresponding Authorization object.
        /// </summary>
        /// <param name="record">The input record.</param>
        /// <returns>The created <see cref="Authorization"/> object.</returns>
        private IEnumerable<Authorization> RetrieveAuthorizations(IEdmVocabularyAnnotatable target)
        {
            IEdmVocabularyAnnotation annotation = Model.GetVocabularyAnnotation(target, Term);
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
