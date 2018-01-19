// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    internal static class EdmModelCapabilitiesHelper
    {
        public static IEnumerable<string> GetAscendingOnlyProperties(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            IEdmRecordExpression record = model.GetSortRestrictionsAnnotation(target);
            if (record != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == "AscendingOnlyProperties");
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null)
                    {
                        foreach(var a in value.Elements.Select(e => e as EdmPropertyPathExpression))
                        {
                            yield return a.Path;
                        }
                    }
                }
            }

            yield return null;
        }

        public static IEnumerable<string> GetDescendingOnlyProperties(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            IEdmRecordExpression record = model.GetSortRestrictionsAnnotation(target);
            if (record != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == "DescendingOnlyProperties");
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null)
                    {
                        foreach (var a in value.Elements.Select(e => e as EdmPropertyPathExpression))
                        {
                            yield return a.Path;
                        }
                    }
                }
            }

            yield return null;
        }

        public static IEnumerable<string> GetNonSortableProperties(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            IEdmRecordExpression record = model.GetSortRestrictionsAnnotation(target);
            if (record != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == "NonSortablePropeties");
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null)
                    {
                        foreach (var a in value.Elements.Select(e => e as EdmPropertyPathExpression))
                        {
                            yield return a.Path;
                        }
                    }
                }
            }

            yield return null;
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

        /// <summary>
        /// Gets description for term Org.OData.Core.V1.Description from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Description for term Org.OData.Core.V1.Description</returns>
        public static IEdmRecordExpression GetSortRestrictionsAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (target == null)
            {
                throw Error.ArgumentNull(nameof(target));
            }

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.SortRestrictions);
            if (term == null)
            {
                return null;
            }

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null)
            {
                IEdmRecordExpression recordExpression = annotation.Value as IEdmRecordExpression;
                if (recordExpression != null)
                {
                    return recordExpression;
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
        public static IEdmRecordExpression GetFilterRestrictionsAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (target == null)
            {
                throw Error.ArgumentNull(nameof(target));
            }

            var a = model.FindDeclaredVocabularyAnnotations(target);

            IEdmTerm term = model.FindTerm("Org.OData.Capabilities.V1.FilterRestrictions");
            if (term == null)
            {
                return null;
            }

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null)
            {
                IEdmRecordExpression recordExpression = annotation.Value as IEdmRecordExpression;
                if (recordExpression != null)
                {
                    return recordExpression;
                }
            }

            return null;
        }



        /// <summary>
        /// Gets bool value for term Org.OData.Core.V1.TopSupported from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Boolean value for term Org.OData.Core.V1.TopSupported</returns>
        public static bool IsTopSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.TopSupported);
            if (term != null)
            {
                bool? boolValue = model.GetBoolean(target, term);
                if (boolValue != null)
                {
                    return boolValue.Value;
                }
            }

            // by default, it supports $top.
            return true;
        }

        /// <summary>
        /// Gets bool value for term Org.OData.Core.V1.SkipSupported from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Boolean value for term Org.OData.Core.V1.SkipSupported</returns>
        public static bool IsSkipSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.SkipSupported);
            if (term != null)
            {
                bool? boolValue = model.GetBoolean(target, term);
                if (boolValue != null)
                {
                    return boolValue.Value;
                }
            }

            // by default, it supports $skip.
            return true;
        }

        /// <summary>
        /// Gets Sortable value for term Org.OData.Core.V1.SortRestrictions from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Boolean value for term Org.OData.Core.V1.SortRestrictions</returns>
        public static bool IsSortable(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.SortRestrictions);
            if (term != null)
            {
                bool? boolValue = model.GetBoolean(target, term, "Sortable");
                if (boolValue != null)
                {
                    return boolValue.Value;
                }
            }

            return true; // by default, it's sortable.
        }

        /// <summary>
        /// Gets Searchable property value for term Org.OData.Core.V1.SearchRestritions from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Boolean value for term Org.OData.Core.V1.SearchRestritions</returns>
        public static bool IsSearchable(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.SearchRestrictions);
            if (term != null)
            {
                bool? boolValue = model.GetBoolean(target, term, "Searchable");
                if (boolValue != null)
                {
                    return boolValue.Value;
                }
            }

            // by default, it supports $search.
            return true;
        }

        /// <summary>
        /// Gets Countable property value for term Org.OData.Core.V1.CountRestrictions from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Boolean value for term Org.OData.Core.V1.CountRestrictions</returns>
        public static bool IsCountable(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.CountRestrictions);
            if (term != null)
            {
                bool? boolValue = model.GetBoolean(target, term, "Countable");
                if (boolValue != null)
                {
                    return boolValue.Value;
                }
            }

            // by default, it supports $count.
            return true;
        }

        /// <summary>
        /// Gets Filterable property value for term Org.OData.Core.V1.FilterRestrictions from a target annotatable
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>Boolean value for term Org.OData.Core.V1.FilterRestrictions</returns>
        public static bool IsFilterable(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            IEdmTerm term = model.FindTerm(CapabilitiesConstants.FilterRestrictions);
            if (term != null)
            {
                bool? boolValue = model.GetBoolean(target, term, "Filterable");
                if (boolValue != null)
                {
                    return boolValue.Value;
                }
            }

            // by default, it supports $filter.
            return true;
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
