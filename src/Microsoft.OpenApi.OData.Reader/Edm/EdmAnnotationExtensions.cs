// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Vocabulary;
using Microsoft.OpenApi.OData.Vocabulary.Authorization;
using Microsoft.OpenApi.OData.Vocabulary.Core;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Vocabulary Annotation Extension methods for <see cref="IEdmModel"/>
    /// </summary>
    internal static class EdmVocabularyAnnotationExtensions
    {
        private static IDictionary<IEdmVocabularyAnnotatable, IDictionary<string, object>> _cachedAnnotations;
        private static IEdmModel _savedModel = null; // if diffenent model, the cache will be cleaned.
        private static object _objectLock = new object();

        /// <summary>
        /// Gets the boolean term value for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="qualifiedName">The Term qualified name.</param>
        /// <returns>Null or the boolean value for this annotation.</returns>
        public static bool? GetBoolean(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            return GetOrAddCached(model, target, qualifiedName, () =>
            {
                bool? value = null;
                IEdmTerm term = model.FindTerm(qualifiedName);
                if (term != null)
                {
                    value = model.GetBoolean(target, term);
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        // Note: Graph has a lot of annotations applied to the type, not to the navigation source.
                        // Here's a work around to retrieve these annotations from type if we can't find it from navigation source.
                        // It's the same reason for belows
                        IEdmNavigationSource navigationSource = target as IEdmNavigationSource;
                        if (navigationSource != null)
                        {
                            IEdmEntityType entityType = navigationSource.EntityType;
                            value = model.GetBoolean(entityType, term);
                        }
                    }
                }

                return value;
            });
        }

        public static bool? GetBoolean(this IEdmModel model, string targetPath, string qualifiedName)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            IEdmTargetPath target = model.GetTargetPath(targetPath);
            if (target == null)
                return default;

            return model.GetBoolean(target, qualifiedName);
        }

        /// <summary>
        /// Gets the string term value for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="qualifiedName">The Term qualified name.</param>
        /// <returns>Null or the string value for this annotation.</returns>
        public static string GetString(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            return GetOrAddCached(model, target, qualifiedName, () =>
            {
                string value = null;
                IEdmTerm term = model.FindTerm(qualifiedName);
                if (term != null)
                {
                    value = model.GetString(target, term);
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        IEdmNavigationSource navigationSource = target as IEdmNavigationSource;
                        if (navigationSource != null)
                        {
                            IEdmEntityType entityType = navigationSource.EntityType;
                            value = model.GetString(entityType, term);
                        }
                    }
                }

                return value;
            });
        }

        /// <summary>
        /// Gets the record value (a complex type) for the given <see cref="IEdmVocabularyAnnotatable"/>
        /// using the default term information assigned to the type.
        /// </summary>
        /// <typeparam name="T">The CLR mapping type.</typeparam>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target element.</param>
        /// <returns>Null or the record value (a complex type) for this annotation.</returns>
        public static T GetRecord<T>(this IEdmModel model, IEdmVocabularyAnnotatable target)
            where T : IRecord, new()
        {
            string qualifiedName = Utils.GetTermQualifiedName<T>();
            return model.GetRecord<T>(target, qualifiedName);
        }

        /// <summary>
        /// Gets the record value (a complex type) for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <typeparam name="T">The CLR mapping type.</typeparam>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="qualifiedName">The Term qualified name.</param>
        /// <returns>Null or the record value (a complex type) for this annotation.</returns>
        public static T GetRecord<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            return GetOrAddCached(model, target, qualifiedName, () =>
            {
                T value = default;
                IEdmTerm term = model.FindTerm(qualifiedName);
                if (term != null)
                {
                    value = model.GetRecord<T>(target, term);
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        IEdmNavigationSource navigationSource = target as IEdmNavigationSource;
                        if (navigationSource != null)
                        {
                            IEdmEntityType entityType = navigationSource.EntityType;
                            value = model.GetRecord<T>(entityType, term);
                        }
                    }
                }

                return value;
            });
        }

        /// <summary>
        /// Gets the record value (a complex type) for the given target path.
        /// </summary>
        /// <typeparam name="T">The CLR mapping type.</typeparam>
        /// <param name="model">The Edm model.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="qualifiedName">The Term qualified name.</param>
        /// <returns></returns>
        public static T GetRecord<T>(this IEdmModel model, string targetPath, string qualifiedName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            IEdmTargetPath target = model.GetTargetPath(targetPath);
            if (target == null)
                return default;

            return model.GetRecord<T>(target, qualifiedName);
        }

        /// <summary>
        /// Gets the collection of string term value for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="qualifiedName">The Term qualified name.</param>
        /// <returns>Null or the collection of string value for this annotation.</returns>
        public static IEnumerable<string> GetCollection(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            return GetOrAddCached(model, target, qualifiedName, () =>
            {
                IEnumerable<string> value = null;
                IEdmTerm term = model.FindTerm(qualifiedName);
                if (term != null)
                {
                    value = model.GetCollection(target, term);
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        IEdmNavigationSource navigationSource = target as IEdmNavigationSource;
                        if (navigationSource != null)
                        {
                            IEdmEntityType entityType = navigationSource.EntityType;
                            value = model.GetCollection(entityType, term);
                        }
                    }
                }

                return value?.ToList();
            });
        }

        /// <summary>
        /// Gets the collection of record value (a complex type) for the given <see cref="IEdmVocabularyAnnotatable"/>
        /// using the default term information assigned to the type.
        /// </summary>
        /// <typeparam name="T">The CLR mapping type.</typeparam>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <returns>Null or the colllection of record value (a complex type) for this annotation.</returns>
        public static IEnumerable<T> GetCollection<T>(this IEdmModel model, IEdmVocabularyAnnotatable target)
            where T : IRecord, new()
        {
            string qualifiedName = Utils.GetTermQualifiedName<T>();
            return GetCollection<T>(model, target, qualifiedName);
        }

        /// <summary>
        /// Gets the collection of record value (a complex type) for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <typeparam name="T">The CLR mapping type.</typeparam>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="qualifiedName">The Term qualified name.</param>
        /// <returns>Null or the colllection of record value (a complex type) for this annotation.</returns>
        public static IEnumerable<T> GetCollection<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            return GetOrAddCached(model, target, qualifiedName, () =>
            {
                IEnumerable<T> value = null;
                IEdmTerm term = model.FindTerm(qualifiedName);
                if (term != null)
                {
                    value = model.GetCollection<T>(target, term);
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        IEdmNavigationSource navigationSource = target as IEdmNavigationSource;
                        if (navigationSource != null)
                        {
                            IEdmEntityType entityType = navigationSource.EntityType;
                            value = model.GetCollection<T>(entityType, term);
                        }
                    }
                }

                return value;
            });
        }

        /// <summary>
        /// Gets the links record value (a complex type) for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <param name="linkRel">The link relation type for path operation.</param>
        /// <returns>Null or the links record value (a complex type) for this annotation.</returns>
        public static LinkType GetLinkRecord(this IEdmModel model, IEdmVocabularyAnnotatable target, string linkRel)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            return model.GetCollection<LinkType>(target, CoreConstants.Links)?.FirstOrDefault(x => x.Rel == linkRel);
        }

        /// <summary>
        /// Gets the links record value (a complex type) for the given target path.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="targetPath">The string representation of the Edm target path.</param>
        /// <param name="linkRel">The link relation type for path operation.</param>
        /// <returns>Null or the links record value (a complex type) for this annotation.</returns>
        public static LinkType GetLinkRecord(this IEdmModel model, string targetPath, string linkRel)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));

            IEdmTargetPath target = model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return model.GetLinkRecord(target, linkRel);
        }

        /// <summary>
        /// Create the corresponding Authorization object.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Edm target.</param>
        /// <returns>The created <see cref="Authorization"/> object.</returns>
        public static IEnumerable<Authorization> GetAuthorizations(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            return GetOrAddCached(model, target, AuthorizationConstants.Authorizations, () =>
            {
                IEdmTerm term = model.FindTerm(AuthorizationConstants.Authorizations);
                if (term != null)
                {
                    IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
                    if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.Collection)
                    {
                        IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;
                        if (collection.Elements != null)
                        {
                            return collection.Elements.Select(e =>
                            {
                                Debug.Assert(e.ExpressionKind == EdmExpressionKind.Record);

                                IEdmRecordExpression recordExpression = (IEdmRecordExpression)e;
                                Authorization auth = Authorization.CreateAuthorization(recordExpression);
                                return auth;
                            });
                        }
                    }
                }

                return null;
            });
        }

        public static string GetDescriptionAnnotation(this IEdmModel model, string targetPath)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(targetPath, nameof(targetPath));

            IEdmTargetPath target = model.GetTargetPath(targetPath);
            if (target == null)
                return null;

            return model.GetDescriptionAnnotation(target);
        }

        private static T GetOrAddCached<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName, Func<T> createFunc)
        {
            if (model == null || target == null)
            {
                return default;
            }

            lock (_objectLock)
            {
                if (!ReferenceEquals(_savedModel, model))
                {
                    if (_cachedAnnotations != null)
                    {
                        _cachedAnnotations.Clear();
                    }

                    _savedModel = model;
                }

                if (_cachedAnnotations == null)
                {
                    _cachedAnnotations = new Dictionary<IEdmVocabularyAnnotatable, IDictionary<string, object>>();
                }

                object restriction;
                if (_cachedAnnotations.TryGetValue(target, out IDictionary<string, object> value))
                {
                    // Here means we visited target before and we are sure that the value is not null.
                    if (value.TryGetValue(qualifiedName, out restriction))
                    {
                        T ret = (T)restriction;
                        return ret;
                    }
                    else
                    {
                        T ret = createFunc();
                        value[qualifiedName] = ret;
                        return ret;
                    }
                }

                // It's first time to query this target, create new dictionary and restriction.
                value = new Dictionary<string, object>();
                _cachedAnnotations[target] = value;
                T newAnnotation = createFunc();
                value[qualifiedName] = newAnnotation;
                return newAnnotation;
            }
        }

        private static bool? GetBoolean(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
        {
            Debug.Assert(model != null);
            Debug.Assert(target != null);
            Debug.Assert(term != null);

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.BooleanConstant)
            {
                IEdmBooleanConstantExpression boolConstant = (IEdmBooleanConstantExpression)annotation.Value;
                if (boolConstant != null)
                {
                    return boolConstant.Value;
                }
            }

            return null;
        }

        private static string GetString(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
        {
            Debug.Assert(model != null);
            Debug.Assert(target != null);
            Debug.Assert(term != null);

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.StringConstant)
            {
                IEdmStringConstantExpression stringConstant = (IEdmStringConstantExpression)annotation.Value;
                return stringConstant.Value;
            }

            return null;
        }

        private static IEnumerable<string> GetCollection(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
        {
            Debug.Assert(model != null);
            Debug.Assert(target != null);
            Debug.Assert(term != null);

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.Collection)
            {
                IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;
                if (collection.Elements != null)
                {
                    return collection.Elements.Select(e => ((IEdmStringConstantExpression)e).Value);
                }
            }

            return null;
        }

        private static T GetRecord<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
            where T : IRecord, new()
        {
            Debug.Assert(model != null);
            Debug.Assert(target != null);
            Debug.Assert(term != null);

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.Record)
            {
                IEdmRecordExpression recordExpression = (IEdmRecordExpression)annotation.Value;
                T newRecord = new T();
                newRecord.Initialize(recordExpression);
                return newRecord;
            }

            return default;
        }

        private static IEnumerable<T> GetCollection<T>(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
            where T : IRecord, new()
        {
            Debug.Assert(model != null);
            Debug.Assert(target != null);
            Debug.Assert(term != null);

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null && annotation.Value != null && annotation.Value.ExpressionKind == EdmExpressionKind.Collection)
            {
                IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;
                if (collection.Elements != null)
                {
                    return collection.Elements.Select(e =>
                    {
                        Debug.Assert(e.ExpressionKind == EdmExpressionKind.Record);

                        IEdmRecordExpression recordExpression = (IEdmRecordExpression)e;
                        T newRecord = new T();
                        newRecord.Initialize(recordExpression);
                        return newRecord;
                    });
                }
            }

            return null;
        }
    }
}
