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
    /// Org.OData.Capabilities.V1.UpdateRestrictions
    /// </summary>
    internal class UpdateRestrictions : CapabilitiesRestrictions
    {
        private bool _updatable = true;
        private IList<string> _nonUpdatableNavigationProperties;

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.UpdateRestrictions;

        /// <summary>
        /// Gets the Updatable value.
        /// </summary>
        public bool Updatable
        {
            get
            {
                Initialize();
                return _updatable;
            }
        }

        /// <summary>
        /// Gets the navigation properties which do not allow rebinding.
        /// </summary>
        public IList<string> NonUpdatableNavigationProperties
        {
            get
            {
                Initialize();
                return _nonUpdatableNavigationProperties;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UpdateRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public UpdateRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input navigation property do not allow rebinding.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonUpdatableNavigationProperty(IEdmNavigationProperty property)
        {
            return NonUpdatableNavigationProperties.Any(a => a == property.Name);
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

            _updatable = SetBoolProperty(record, "Updatable", true);

            _nonUpdatableNavigationProperties = GetCollectNavigationProperty(record, "NonUpdatableNavigationProperties");
        }
    }
}
