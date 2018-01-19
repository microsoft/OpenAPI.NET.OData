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
    /// Org.OData.Capabilities.V1.InsertRestrictions
    /// </summary>
    internal class InsertRestrictions : CapabilitiesRestrictions
    {
        private bool _insertable = true;
        private IList<string> _nonInsertableNavigationProperties = new List<string>();

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.InsertRestrictions;

        /// <summary>
        /// Gets the Insertable value.
        /// </summary>
        public bool Insertable
        {
            get
            {
                Initialize();
                return _insertable;
            }
        }

        /// <summary>
        /// Gets the navigation properties which do not allow deep inserts.
        /// </summary>
        public IList<string> NonInsertableNavigationProperties
        {
            get
            {
                Initialize();
                return _nonInsertableNavigationProperties;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="InsertRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public InsertRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input navigation property do not allow deep insert.
        /// </summary>
        /// <param name="property">The input property.</param>
        /// <returns>True/False.</returns>
        public bool IsNonINsertableNavigationProperty(IEdmNavigationProperty property)
        {
            return NonInsertableNavigationProperties.Any(a => a == property.Name);
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

            _insertable = SetBoolProperty(record, "Insertable", true);

            _nonInsertableNavigationProperties = GetCollectNavigationProperty(record, "NonInsertableNavigationProperties");
        }
    }
}
