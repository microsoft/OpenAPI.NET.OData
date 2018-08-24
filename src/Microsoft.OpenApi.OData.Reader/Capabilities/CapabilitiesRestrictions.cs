// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// The base class of Capabilities
    /// </summary>
    internal abstract class CapabilitiesRestrictions : ICapablitiesRestrictions
    {
        /// <summary>
        /// The Capablities Kind.
        /// </summary>
        public abstract CapabilitesTermKind Kind { get; }

        /// <summary>
        /// Load the annotation value.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        public virtual bool Load(IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            string termQualifiedName = CapabilitiesConstants.Namespace + "." + Kind.ToString();
            IEdmVocabularyAnnotation annotation = model.GetVocabularyAnnotation(target, termQualifiedName);
            if (annotation == null)
            {
                IEdmNavigationSource navigationSource = target as IEdmNavigationSource;

                // if not, search the entity type.
                if (navigationSource != null)
                {
                    IEdmEntityType entityType = navigationSource.EntityType();
                    annotation = model.GetVocabularyAnnotation(entityType, termQualifiedName);
                }
            }

            return Initialize(annotation);
        }

        /// <summary>
        /// Initialize the capabilities with the vocabulary annotation.
        /// </summary>
        /// <param name="annotation">The input vocabulary annotation.</param>
        protected abstract bool Initialize(IEdmVocabularyAnnotation annotation);
    }
}
