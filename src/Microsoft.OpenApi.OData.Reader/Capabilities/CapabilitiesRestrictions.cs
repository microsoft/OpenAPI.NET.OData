// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// The base class of Capabilities
    /// </summary>
    internal abstract class CapabilitiesRestrictions
    {
        /// <summary>
        /// Gets the <see cref="IEdmModel"/>.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        public IEdmVocabularyAnnotatable Target { get; }

        /// <summary>
        /// The Term qualified name.
        /// </summary>
        public virtual string QualifiedName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CapabilitiesRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm vocabulary annotatable.</param>
        public CapabilitiesRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            Model = model;
            Target = target;
            Initialize();
        }

        protected void Initialize()
        {
            IEdmVocabularyAnnotation annotation = Model.GetCapabilitiesAnnotation(Target, QualifiedName);
            if (annotation == null)
            {
                IEdmNavigationSource navigationSource = Target as IEdmNavigationSource;

                // if not, search the entity type.
                if (navigationSource != null)
                {
                    IEdmEntityType entityType = navigationSource.EntityType();
                    annotation = Model.GetCapabilitiesAnnotation(entityType, QualifiedName);
                }
            }

            Initialize(annotation);
        }

        /// <summary>
        /// Initialize the capabilities with the vocabulary annotation.
        /// </summary>
        /// <param name="annotation">The input vocabulary annotation.</param>
        protected abstract void Initialize(IEdmVocabularyAnnotation annotation);
    }
}
