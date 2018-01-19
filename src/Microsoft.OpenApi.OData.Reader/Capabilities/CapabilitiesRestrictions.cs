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
    /// The base class of Capabilities
    /// </summary>
    internal abstract class CapabilitiesRestrictions
    {
        private bool _initialized;

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
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Target = target ?? throw Error.ArgumentNull(nameof(target));
        }

        protected void Initialize()
        {
            if (_initialized)
            {
                return;
            }

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
            _initialized = true;
        }

        protected abstract void Initialize(IEdmVocabularyAnnotation annotation);

        protected static bool SetBoolProperty(IEdmRecordExpression record, string propertyName, bool defaultValue)
        {
            if (record == null || record.Properties == null)
            {
                return defaultValue;
            }

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmBooleanConstantExpression value = property.Value as IEdmBooleanConstantExpression;
                if (value != null)
                {
                    return value.Value;
                }
            }

            return defaultValue;
        }

        protected static bool? SetBoolProperty(IEdmRecordExpression record, string propertyName)
        {
            if (record == null || record.Properties == null)
            {
                return null;
            }

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmBooleanConstantExpression value = property.Value as IEdmBooleanConstantExpression;
                if (value != null)
                {
                    return value.Value;
                }
            }

            return null;
        }

        protected static IList<string> GetCollectProperty(IEdmRecordExpression record, string propertyName)
        {
            IList<string> properties = new List<string>();
            if (record != null && record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null)
                    {
                        foreach (var a in value.Elements.Select(e => e as EdmPropertyPathExpression))
                        {
                            properties.Add(a.Path);
                        }
                    }
                }
            }

            return properties;
        }

        public IList<string> GetCollectNavigationProperty(IEdmRecordExpression record, string propertyName)
        {
            IList<string> properties = new List<string>();
            if (record != null && record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null)
                    {
                        foreach (var a in value.Elements.Select(e => e as EdmNavigationPropertyPathExpression))
                        {
                            properties.Add(a.Path);
                        }
                    }
                }
            }

            return properties;
        }
    }
}
