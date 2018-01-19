// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Org.OData.Capabilities.V1.DeleteRestrictions
    /// </summary>
    internal class DeleteRestrictions : CapabilitiesRestrictions
    {
        private bool _deletable = true;
        private IList<string> _nonDeletableNavigationProperties;

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.DeleteRestrictions;

        /// <summary>
        /// Gets the Deletable value.
        /// </summary>
        public bool Deletable
        {
            get
            {
                Initialize();
                return _deletable;
            }
        }

        /// <summary>
        /// Gets the navigation properties which do not allow DeleteLink requests.
        /// </summary>
        public IList<string> NonDeletableNavigationProperties
        {
            get
            {
                Initialize();
                return _nonDeletableNavigationProperties;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeleteRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public DeleteRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input navigation property do not allow DeleteLink requests.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool NonDeletableNavigationProperty(IEdmNavigationProperty property)
        {
            return NonDeletableNavigationProperties.Any(a => a == property.Name);
        }

        protected override void Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return;
            }

            IEdmRecordExpression record = (IEdmRecordExpression)annotation.Value;

            _deletable = SetBoolProperty(record, "Deletable", true);

            _nonDeletableNavigationProperties = GetCollectNavigationProperty(record, "NonDeletableNavigationProperties");
        }
    }
}
