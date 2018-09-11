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

namespace Microsoft.OpenApi.OData.Annotations
{
    /// <summary>
    /// Org.OData.Core.V1.HttpRequests provider
    /// </summary>
    internal class HttpRequestProvider
    {
        /// <summary>
        /// Annotatable: EntitySet Singleton ActionImport FunctionImport Action Function
        /// Collection(Core.HttpRequest)
        /// </summary>
        private IDictionary<IEdmVocabularyAnnotatable, IList<HttpRequest>> _requests
            = new Dictionary<IEdmVocabularyAnnotatable, IList<HttpRequest>>();

        /// <summary>
        /// Gets the Edm mode.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the Edm Term.
        /// </summary>
        public IEdmTerm Term { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="HttpRequestProvider"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public HttpRequestProvider(IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            Term = model.FindTerm("Org.OData.Core.V1.HttpRequests");

            Model = model;
        }

        /// <summary>
        /// Gets Org.OData.Core.V1.HttpRequest.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="method">The request method.</param>
        /// <returns>The Org.OData.Core.V1.HttpRequest or null.</returns>
        public HttpRequest GetHttpRequest(IEdmVocabularyAnnotatable target, string method)
        {
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(method, nameof(method));

            var requests = GetHttpRequests(target);
            return requests?.FirstOrDefault(e => string.Equals(e.MethodType, method, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the collection of Org.OData.Core.V1.HttpRequest for a given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>The collection of Org.OData.Core.V1.HttpRequest</returns>
        public IEnumerable<HttpRequest> GetHttpRequests(IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(target, nameof(target));

            if (Term == null)
            {
                return null;
            }

            // Search the cache.
            if (_requests.TryGetValue(target, out IList<HttpRequest> value))
            {
                return value;
            }

            IEdmVocabularyAnnotation annotation = Model.GetVocabularyAnnotation(target, Term);
            if (annotation == null)
            {
                IEdmNavigationSource navigationSource = target as IEdmNavigationSource;

                // if not, search the entity type.
                if (navigationSource != null)
                {
                    IEdmEntityType entityType = navigationSource.EntityType();
                    annotation = Model.GetVocabularyAnnotation(entityType, Term);
                }
            }

            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Collection)
            {
                _requests[target] = null;
                return null;
            }

            if (target is IEdmEntitySet)
            {
                IEdmEntitySet entitySet = target as IEdmEntitySet;
                if (entitySet.Name == "domains")
                {
                    int pp = 0;
                }
            }

            IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;
            var httpRequests = new List<HttpRequest>();
            foreach (var item in collection.Elements)
            {
                IEdmRecordExpression record = (IEdmRecordExpression)item;
                HttpRequest newRequest = new HttpRequest();
                newRequest.Init(record);
                httpRequests.Add(newRequest);
            }

            _requests[target] = httpRequests;
            return httpRequests;
        }
    }
}
