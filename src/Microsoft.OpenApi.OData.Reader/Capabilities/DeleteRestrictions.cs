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
        /// <summary>
        /// The Term type kind.
        /// </summary>
        public override CapabilitesTermKind Kind  => CapabilitesTermKind.DeleteRestrictions;

        /// <summary>
        /// Gets the Deletable value.
        /// </summary>
        public bool? Deletable { get; private set; }

        /// <summary>
        /// Gets the navigation properties which do not allow DeleteLink requests.
        /// </summary>
        public IList<string> NonDeletableNavigationProperties { get; private set; }

        /// <summary>
        /// Test the target supports delete.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool IsDeletable => Deletable == null || Deletable.Value;

        /// <summary>
        /// Test the input navigation property do not allow DeleteLink requests.
        /// </summary>
        /// <param name="navigationPropertyPath">The input navigation property path.</param>
        /// <returns>True/False.</returns>
        public bool IsNonDeletableNavigationProperty(string navigationPropertyPath)
        {
            return NonDeletableNavigationProperties != null ?
                NonDeletableNavigationProperties.Any(a => a == navigationPropertyPath) :
                false;
        }

        protected override bool Initialize(IEdmVocabularyAnnotation annotation)
        {
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Record)
            {
                return false;
            }

            IEdmRecordExpression record = (IEdmRecordExpression)annotation.Value;

            // Deletable
            Deletable = record.GetBoolean("Deletable");

            // NonDeletableNavigationProperties
            NonDeletableNavigationProperties = record.GetCollectionPropertyPath("NonDeletableNavigationProperties");

            return true;
        }
    }
}
