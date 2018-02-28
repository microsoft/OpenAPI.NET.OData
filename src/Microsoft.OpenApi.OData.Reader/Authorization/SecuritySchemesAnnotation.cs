// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Authorization
{
    /// <summary>
    /// Constant values for Authorization Vocabulary
    /// </summary>
    internal class SecuritySchemesAnnotation
    {
        public IEdmModel Model { get; }

        public IEdmVocabularyAnnotatable Target { get; }

        public IList<SecurityScheme> SecuritySchemes { get; private set; }

        public SecuritySchemesAnnotation(IEdmModel model, IEdmEntityContainer container)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(container, nameof(container));

            Model = model;
            Target = container;
            Initialize();
        }

        protected virtual void Initialize()
        {
            IEdmVocabularyAnnotation annotation = Model.GetVocabularyAnnotation(Target, AuthorizationConstants.SecuritySchemes);
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Collection)
            {
                return;
            }

            IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;

            SecuritySchemes = new List<SecurityScheme>();
            foreach (var item in collection.Elements)
            {
                IEdmRecordExpression record = (IEdmRecordExpression)item;

             //   Authorization auth = new Authorization();
               // auth.Init(record);
                //Authorizations.Add(auth);
            }
        }
    }
}