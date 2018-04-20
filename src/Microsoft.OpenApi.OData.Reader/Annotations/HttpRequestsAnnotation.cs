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
    /// Org.OData.Core.V1.HttpRequests
    /// </summary>
    internal class HttpRequestsAnnotation
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public virtual string QualifiedName => "Org.OData.Core.V1.HttpRequests";

        /// <summary>
        /// Gets the http request array.
        /// </summary>
        public IList<HttpRequest> Requests { get; private set; }

        /// <summary>
        /// Gets the Edm mode.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the vocabulary annotatble.
        /// </summary>
        public IEdmVocabularyAnnotatable Target { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="HttpRequestsAnnotation"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public HttpRequestsAnnotation(IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            Model = model;
            Target = target;

            Initialize();
        }

        public HttpRequest GetRequest(string method)
        {
            if (Requests == null)
            {
                return null;
            }

            return Requests.FirstOrDefault(e => string.Equals(e.MethodType, method, System.StringComparison.OrdinalIgnoreCase));
        }

        protected virtual void Initialize()
        {
            IEdmVocabularyAnnotation annotation = Model.GetVocabularyAnnotation(Target, QualifiedName);
            if (annotation == null)
            {
                IEdmNavigationSource navigationSource = Target as IEdmNavigationSource;

                // if not, search the entity type.
                if (navigationSource != null)
                {
                    IEdmEntityType entityType = navigationSource.EntityType();
                    annotation = Model.GetVocabularyAnnotation(entityType, QualifiedName);
                }
            }

            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Collection)
            {
                return;
            }

            IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;

            Requests = new List<HttpRequest>();
            foreach (var item in collection.Elements)
            {
                IEdmRecordExpression record = (IEdmRecordExpression)item;
                HttpRequest request = new HttpRequest();
                request.Init(record);
                Requests.Add(request);
            }
        }
    }
}
