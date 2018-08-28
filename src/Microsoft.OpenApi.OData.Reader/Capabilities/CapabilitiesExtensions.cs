// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Properties;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// The class provides the functionality for the capabilities annotation.
    /// </summary>
    internal static class CapabilitiesExtensions
    {
        private static IDictionary<IEdmVocabularyAnnotatable, IDictionary<CapabilitesTermKind, ICapablitiesRestrictions>> _capabilitesRestrictions;
        private static IEdmModel _savedModel = null;
        private static object _objectLock = new object();

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.SearchRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.SearchRestrictions or null.</returns>
        public static SearchRestrictions GetSearchRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.SearchRestrictions) as SearchRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.FilterRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.FilterRestrictions or null.</returns>
        public static FilterRestrictions GetFilterRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.FilterRestrictions) as FilterRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.NavigationRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.NavigationRestrictions or null.</returns>
        public static NavigationRestrictions GetNavigationRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.NavigationRestrictions) as NavigationRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.ExpandRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.ExpandRestrictions or null.</returns>
        public static ExpandRestrictions GetExpandRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.ExpandRestrictions) as ExpandRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.DeleteRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.DeleteRestrictions or null.</returns>
        public static DeleteRestrictions GetDeleteRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.DeleteRestrictions) as DeleteRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.UpdateRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.UpdateRestrictions or null.</returns>
        public static UpdateRestrictions GetUpdateRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.UpdateRestrictions) as UpdateRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.InsertRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.InsertRestrictions or null.</returns>
        public static InsertRestrictions GetInsertRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.InsertRestrictions) as InsertRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.SortRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.SortRestrictions or null.</returns>
        public static SortRestrictions GetSortRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.SortRestrictions) as SortRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.CountRestrictions.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.CountRestrictions or null.</returns>
        public static CountRestrictions GetCountRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.CountRestrictions) as CountRestrictions;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.BatchSupported.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.BatchSupported or null.</returns>
        public static BatchSupported GetBatchSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.BatchSupported) as BatchSupported;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.SkipSupported.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.SkipSupported or null.</returns>
        public static SkipSupported GetSkipSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.SkipSupported) as SkipSupported;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.TopSupported.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.TopSupported or null.</returns>
        public static TopSupported GetTopSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.TopSupported) as TopSupported;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.KeyAsSegmentSupported.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.KeyAsSegmentSupported or null.</returns>
        public static KeyAsSegmentSupported GetKeyAsSegmentSupported(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.KeyAsSegmentSupported) as KeyAsSegmentSupported;
        }

        /// <summary>
        /// Gets Org.OData.Capabilities.V1.IndexableByKey.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <returns>The Org.OData.Capabilities.V1.IndexableByKey or null.</returns>
        public static IndexableByKey GetIndexableByKey(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            return model.GetCapabilities(target, CapabilitesTermKind.IndexableByKey) as IndexableByKey;
        }

        /// <summary>
        /// Create the capabiliites restriction
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The Target.</param>
        /// <param name="kind">The Capabiliites kind.</param>
        /// <returns>The <see cref="ICapablitiesRestrictions"/>.</returns>
        public static ICapablitiesRestrictions CreateCapabilitesRestrictions(this IEdmModel model, IEdmVocabularyAnnotatable target, CapabilitesTermKind kind)
        {
            Debug.Assert(model != null);
            Debug.Assert(target != null);

            ICapablitiesRestrictions capabilitiesRestrictions = null;
            switch(kind)
            {
                case CapabilitesTermKind.DeleteRestrictions: // DeleteRestrictions
                    capabilitiesRestrictions = new DeleteRestrictions();
                    break;

                case CapabilitesTermKind.UpdateRestrictions: // UpdateRestrictions
                    capabilitiesRestrictions = new UpdateRestrictions();
                    break;

                case CapabilitesTermKind.InsertRestrictions: // InsertRestrictions
                    capabilitiesRestrictions = new InsertRestrictions();
                    break;

                case CapabilitesTermKind.SearchRestrictions: // SearchRestrictions
                    capabilitiesRestrictions = new SearchRestrictions();
                    break;

                case CapabilitesTermKind.ExpandRestrictions: // ExpandRestrictions
                    capabilitiesRestrictions = new ExpandRestrictions();
                    break;

                case CapabilitesTermKind.SortRestrictions: // SortRestrictions
                    capabilitiesRestrictions = new SortRestrictions();
                    break;

                case CapabilitesTermKind.FilterRestrictions: // FilterRestrictions
                    capabilitiesRestrictions = new FilterRestrictions();
                    break;

                case CapabilitesTermKind.NavigationRestrictions: // NavigationRestrictions
                    capabilitiesRestrictions = new NavigationRestrictions();
                    break;

                case CapabilitesTermKind.CountRestrictions: // CountRestrictions
                    capabilitiesRestrictions = new CountRestrictions();
                    break;

                case CapabilitesTermKind.BatchSupported: // BatchSupported
                    capabilitiesRestrictions = new BatchSupported();
                    break;

                case CapabilitesTermKind.SkipSupported: // SkipSupported
                    capabilitiesRestrictions = new SkipSupported();
                    break;

                case CapabilitesTermKind.TopSupported: // TopSupported
                    capabilitiesRestrictions = new TopSupported();
                    break;

                case CapabilitesTermKind.KeyAsSegmentSupported: // KeyAsSegmentSupported
                    capabilitiesRestrictions = new KeyAsSegmentSupported();
                    break;

                case CapabilitesTermKind.IndexableByKey: // IndexableByKey
                    capabilitiesRestrictions = new IndexableByKey();
                    break;

                case CapabilitesTermKind.ChangeTracking: // ChangeTracking
                case CapabilitesTermKind.CrossJoinSupported: // CrossJoinSupported
                case CapabilitesTermKind.CallbackSupported: // CallbackSupported
                case CapabilitesTermKind.FilterFunctions: // FilterFunctions
                case CapabilitesTermKind.BatchContinueOnErrorSupported: // BatchContinueOnErrorSupported
                case CapabilitesTermKind.AsynchronousRequestsSupported: // AsynchronousRequestsSupported
                case CapabilitesTermKind.Isolation: // Isolation
                case CapabilitesTermKind.AcceptableEncodings: // AcceptableEncodings
                case CapabilitesTermKind.SupportedFormats: // SupportedFormats
                default:
                    throw Error.NotSupported(String.Format(SRResource.CapabilitiesKindNotSupported, kind));
            }

            // load the annotation value
            if (!capabilitiesRestrictions.Load(model, target))
            {
                return null;
            }

            return capabilitiesRestrictions;
        }

        /// <summary>
        /// Gets the capablities from the <see cref="IEdmModel"/> for the given <see cref="IEdmVocabularyAnnotatable"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The target.</param>
        /// <param name="kind">Thye Capabilites kind.</param>
        /// <returns>The capabilities restrictions or null.</returns>
        private static ICapablitiesRestrictions GetCapabilities(this IEdmModel model, IEdmVocabularyAnnotatable target, CapabilitesTermKind kind)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));

            lock (_objectLock)
            {
                if (!ReferenceEquals(_savedModel, model))
                {
                    if (_capabilitesRestrictions != null)
                    {
                        _capabilitesRestrictions.Clear();
                    }
                    _savedModel = model;
                }

                if (_capabilitesRestrictions == null)
                {
                    _capabilitesRestrictions = new Dictionary<IEdmVocabularyAnnotatable, IDictionary<CapabilitesTermKind, ICapablitiesRestrictions>>();
                }

                ICapablitiesRestrictions restriction;
                if (_capabilitesRestrictions.TryGetValue(target, out IDictionary<CapabilitesTermKind, ICapablitiesRestrictions> value))
                {
                    // Here means we visited target before and we are sure that the value is not null.
                    if (value.TryGetValue(kind, out restriction))
                    {
                        return restriction;
                    }
                    else
                    {
                        restriction = CreateCapabilitesRestrictions(model, target, kind);
                        value[kind] = restriction;
                        return restriction;
                    }
                }

                // It's first time to query this target, create new dictionary and restriction.
                value = new Dictionary<CapabilitesTermKind, ICapablitiesRestrictions>();
                _capabilitesRestrictions[target] = value;
                restriction = CreateCapabilitesRestrictions(model, target, kind);
                value[kind] = restriction;
                return restriction;
            }
        }
    }
}
