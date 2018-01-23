// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
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
    /// Complex type name = NavigationPropertyRestriction
    /// </summary>
    internal class NavigationPropertyRestriction
    {
        /// <summary>
        /// Navigation properties can be navigated
        /// </summary>
        public string NavigationProperty { get; set; }

        /// <summary>
        /// Navigation properties can be navigated to this level.
        /// </summary>
        public NavigationType? Navigability { get; set; }
    }

    /// <summary>
    /// Org.OData.Capabilities.V1.NavigationRestrictions
    /// </summary>
    internal class NavigationRestrictions : CapabilitiesRestrictions
    {
        /// <summary>
        /// The Term type name.
        /// </summary>
        public override string QualifiedName => CapabilitiesConstants.NavigationRestrictions;

        /// <summary>
        /// Gets the Navigability value.
        /// </summary>
        public NavigationType? Navigability { get; private set; }

        /// <summary>
        /// Gets the navigation properties which has navigation restrictions.
        /// </summary>
        public IList<NavigationPropertyRestriction> RestrictedProperties { get; private set; }

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
        /// Test the input navigation property which has navigation restrictions.
        /// </summary>
        /// <param name="property">The input navigation property.</param>
        /// <returns>True/False.</returns>
        public bool IsRestrictedProperty(IEdmNavigationProperty property)
        {
            return RestrictedProperties != null ?
                RestrictedProperties.Where(a => a.NavigationProperty == property.Name)
                .Any(a => a.Navigability != null && a.Navigability.Value == NavigationType.None) :
                true;
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

            // Navigability
            Navigability = record.GetEnum<NavigationType>("Navigability");

            // RestrictedProperties
            RestrictedProperties = GetRestrictedProperties(record);
        }

        private static IList<NavigationPropertyRestriction> GetRestrictedProperties(IEdmRecordExpression record)
        {
            if (record != null && record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(p => p.Name == "RestrictedProperties");
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null && value.Elements != null)
                    {
                        IList<NavigationPropertyRestriction> restrictedProperties = new List<NavigationPropertyRestriction>();
                        foreach (var item in value.Elements.OfType<IEdmRecordExpression>())
                        {
                            NavigationPropertyRestriction restriction = new NavigationPropertyRestriction();
                            restriction.Navigability = item.GetEnum<NavigationType>("Navigability");
                            restriction.NavigationProperty = item.GetPropertyPath("NavigationProperty");
                            restrictedProperties.Add(restriction);
                        }

                        if (restrictedProperties.Any())
                        {
                            return restrictedProperties;
                        }
                    }
                }
            }

            return null;
        }
    }
}
