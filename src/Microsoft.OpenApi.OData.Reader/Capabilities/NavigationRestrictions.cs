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
    /// Enumerates the navigation type can apply on navigation restrictions.
    /// </summary>
    internal enum NavigationType
    {
        /// <summary>
        /// Navigation properties can be recursively navigated.
        /// </summary>
        Recursive,

        /// <summary>
        /// Navigation properties can be navigated to a single level.
        /// </summary>
        Single,

        /// <summary>
        /// Navigation properties are not navigable.
        /// </summary>
        None
    }

    /// <summary>
    /// Org.OData.Capabilities.V1.NavigationRestrictions
    /// </summary>
    internal class NavigationRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// Complex type name = NavigationPropertyRestriction
        /// </summary>
        public class NavigationPropertyRestriction
        {
            /// <summary>
            /// Navigation properties can be navigated
            /// </summary>
            public string NavigationProperty { get; set; }

            /// <summary>
            /// Navigation properties can be navigated to this level.
            /// </summary>
            public NavigationType Navigability { get; set; }
        }

        private NavigationType _navigability = NavigationType.Single;
        private IList<NavigationPropertyRestriction> _restrictedProperties = new List<NavigationPropertyRestriction>();

        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.NavigationRestrictions;

        /// <summary>
        /// Gets the Navigability value.
        /// </summary>
        public NavigationType Navigability
        {
            get
            {
                Initialize();
                return _navigability;
            }
        }

        /// <summary>
        /// Gets the navigation properties which has navigation restrictions.
        /// </summary>
        public IList<NavigationPropertyRestriction> RestrictedProperties
        {
            get
            {
                Initialize();
                return _restrictedProperties;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NavigationRestrictions"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm annotation target.</param>
        public NavigationRestrictions(IEdmModel model, IEdmVocabularyAnnotatable target)
            : base(model, target)
        {
        }

        /// <summary>
        /// Test the input property cannot be used in $orderby expressions.
        /// </summary>
        /// <param name="property">The input property name.</param>
        /// <returns>True/False.</returns>
        public bool IsNonNavigationProperty(IEdmNavigationProperty property)
        {
            return RestrictedProperties
                .Where(a => a.NavigationProperty == property.Name)
                .Any(a => a.Navigability == NavigationType.None);
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

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == "Navigability");
            if (property != null)
            {
                IEdmEnumMemberExpression value = property.Value as IEdmEnumMemberExpression;
                if (value != null)
                {
                    // Expandable = value.EnumMembers.First().Value.Value;
                }
            }

            property = record.Properties.FirstOrDefault(e => e.Name == "RestrictedProperties");
            if (property != null)
            {
                IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                if (value != null)
                {
                    foreach (var a in value.Elements.Select(e => e as EdmNavigationPropertyPathExpression))
                    {
                        //NonExpandableProperties.Add(a.Path);
                    }
                }
            }
        }
    }
}
