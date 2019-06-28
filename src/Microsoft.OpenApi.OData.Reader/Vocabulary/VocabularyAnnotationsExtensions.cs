// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Vocabulary
{
    /// <summary>
    /// Constant values for Vocabulary
    /// </summary>
    internal static class VocabularyAnnotationsExtensions
    {
        /// <summary>
        /// Gets Org.OData.Capabilities.V1.SearchRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.SearchRestrictions or null.</returns>
        public static T GetRestrictions<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
            where T : IRecord, new()
        {
            return model.GetRestrictionsImpl<T>(target, qualifiedName);
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.TopSupported.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.TopSupported or null.</returns>
        public static bool? GetTopSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return GetSupported(model, target, CapabilitiesConstants.TopSupported);
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.SkipSupported.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.SkipSupported or null.</returns>
        public static bool? GetSkipSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return GetSupported(model, target, CapabilitiesConstants.SkipSupported);
        }

        private static bool? GetSupported(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
        {
            bool? supported = model.GetBoolean(target, qualifiedName);
            if (supported == null)
            {
                IEdmNavigationSource navigationSource = target as IEdmNavigationSource;

                // if not, search the entity type.
                if (navigationSource != null)
                {
                    IEdmEntityType entityType = navigationSource.EntityType();
                    supported = model.GetBoolean(entityType, qualifiedName);
                }
            }

            return supported;
        }

        private static T GetRestrictionsImpl<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
            where T : IRecord, new()
        {
            T restrictions = model.GetRecord<T>(target, qualifiedName);
            if (restrictions == null)
            {
                IEdmNavigationSource navigationSource = target as IEdmNavigationSource;

                // if not, search the entity type.
                if (navigationSource != null)
                {
                    IEdmEntityType entityType = navigationSource.EntityType();
                    restrictions = model.GetRecord<T>(entityType, qualifiedName);
                }
            }

            return restrictions;
        }
    }
}
