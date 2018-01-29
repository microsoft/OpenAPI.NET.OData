// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    internal static class CapabilitiesHelper
    {
        /// <summary>
        /// Gets boolean value for term Org.OData.Core.V1.KeyAsSegmentSupported
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <returns>Boolean for term Org.OData.Core.V1.KeyAsSegmentSupported</returns>
        public static bool GetKeyAsSegmentSupported(this IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            if (model.EntityContainer == null)
            {
                return false;
            }

            KeyAsSegmentSupported keyAsSegment = new KeyAsSegmentSupported(model, model.EntityContainer);
            return keyAsSegment.Supported ?? false;
        }

        /// <summary>
        /// Gets description for term Org.OData.Core.V1.Description from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Description for term Org.OData.Core.V1.Description</returns>
        public static IEdmVocabularyAnnotation GetCapabilitiesAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            IEdmTerm term = model.FindTerm(qualifiedName);
            if (term != null)
            {
                IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
                if (annotation != null)
                {
                    return annotation;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets description for term Org.OData.Core.V1.Description from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Description for term Org.OData.Core.V1.Description</returns>
        public static IEdmVocabularyAnnotation GetCapabilitiesAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(term, nameof(term));

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null)
            {
                return annotation;
            }

            return null;
        }

        private static bool? GetBoolean(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term, string propertyName = null)
        {
            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null)
            {
                switch (annotation.Value.ExpressionKind)
                {
                    case EdmExpressionKind.Record:
                        IEdmRecordExpression recordExpression = (IEdmRecordExpression)annotation.Value;
                        if (recordExpression != null)
                        {
                            IEdmPropertyConstructor property = recordExpression.Properties.FirstOrDefault(e => e.Name == propertyName);
                            if (property != null)
                            {
                                IEdmBooleanConstantExpression value = property.Value as IEdmBooleanConstantExpression;
                                if (value != null)
                                {
                                    return value.Value;
                                }
                            }
                        }
                        break;

                    case EdmExpressionKind.BooleanConstant:
                        IEdmBooleanConstantExpression boolConstant = (IEdmBooleanConstantExpression)annotation.Value;
                        if (boolConstant != null)
                        {
                            return boolConstant.Value;
                        }
                        break;
                }
            }

            return null;
        }
    }
}
