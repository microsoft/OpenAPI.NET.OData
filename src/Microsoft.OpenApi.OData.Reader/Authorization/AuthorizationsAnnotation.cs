// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using System.Collections.Generic;

namespace Microsoft.OpenApi.OData.Authorization
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal class AuthorizationsAnnotation
    {
        public IEdmModel Model { get; }

        public IEdmVocabularyAnnotatable Target { get; }

        public IList<Authorization> Authorizations { get; private set; }

        public AuthorizationsAnnotation(IEdmModel model, IEdmEntityContainer container)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(container, nameof(container));

            Model = model;
            Target = container;
            Initialize();
        }

        protected virtual void Initialize()
        {
            IEdmVocabularyAnnotation annotation = Model.GetVocabularyAnnotation(Target, AuthorizationConstants.Authorizations);
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Collection)
            {
                return;
            }

            IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;

            Authorizations = new List<Authorization>();
            foreach (var item in collection.Elements)
            {
                IEdmRecordExpression record = (IEdmRecordExpression)item;
                if (record.DeclaredType == null)
                {
                    continue;
                }

             //   Authorization auth = new Authorization();
               // auth.Init(record);
                //Authorizations.Add(auth);
            }
        }
    }
}