﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmNavigationProperty"/>.
    /// </summary>
    internal class NavigationPropertyPathItemHandler : PathItemHandler
    {
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.NavigationProperty;

        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a key segment.
        /// </summary>
        protected bool LastSegmentIsKeySegment { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a $ref segment.
        /// </summary>
        protected bool LastSegmentIsRefSegment { get; private set; }

        /// <summary>
        /// The entity type targeted by the <see cref="NavigationProperty"/>
        /// </summary>
        private IEdmEntityType _navPropEntityType;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            IEdmEntitySet entitySet = NavigationSource as IEdmEntitySet;
            IEdmVocabularyAnnotatable target = entitySet;
            if (target == null)
            {
                target = NavigationSource as IEdmSingleton;
            }                       

            NavigationRestrictionsType navSourceRestrictionType = Context.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
            NavigationRestrictionsType navPropRestrictionType = Context.Model.GetRecord<NavigationRestrictionsType>(NavigationProperty, CapabilitiesConstants.NavigationRestrictions);           
           
            NavigationPropertyRestriction restriction = navSourceRestrictionType?.RestrictedProperties?
                .FirstOrDefault(r => r.NavigationProperty == Path.NavigationPropertyPath())
                ?? navPropRestrictionType?.RestrictedProperties?.FirstOrDefault();

            // Check whether the navigation property should be part of the path
            if (EdmModelHelper.NavigationRestrictionsAllowsNavigability(navSourceRestrictionType, restriction) == false ||
                EdmModelHelper.NavigationRestrictionsAllowsNavigability(navPropRestrictionType, restriction) == false)
            {
                return;
            }

            // containment: Get / (Post - Collection | Patch - Single)
            // non-containment: Get
            AddGetOperation(item, restriction);

            if (NavigationProperty.ContainsTarget)
            {
                if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    if (LastSegmentIsKeySegment)
                    {
                        UpdateRestrictionsType entityUpdateRestriction = Context.Model.GetRecord<UpdateRestrictionsType>(_navPropEntityType);
                        UpdateRestrictionsType navPropUpdateRestriction = Context.Model.GetRecord<UpdateRestrictionsType>(NavigationProperty);
                        if ((entityUpdateRestriction?.IsUpdatable ?? true) &&
                            (navPropUpdateRestriction?.IsUpdatable ?? true))
                        {
                            UpdateRestrictionsType updateRestrictionType = navPropUpdateRestriction ?? entityUpdateRestriction;
                            AddUpdateOperation(item, restriction, updateRestrictionType);
                        }
                    }
                    else
                    {
                        InsertRestrictionsType navPropInsertRestriction = restriction?.InsertRestrictions ??
                            Context.Model.GetRecord<InsertRestrictionsType>(NavigationProperty);
                        InsertRestrictionsType entityInsertRestriction = Context.Model.GetRecord<InsertRestrictionsType>(_navPropEntityType);

                        bool isInsertableDefault = navPropInsertRestriction == null && entityInsertRestriction == null;

                        if (isInsertableDefault ||
                           ((entityInsertRestriction?.IsInsertable ?? true) &&
                           (navPropInsertRestriction?.IsInsertable ?? true)))
                        {
                            AddOperation(item, OperationType.Post);
                        }
                    }
                }
                else
                {
                    AddUpdateOperation(item, restriction);
                }
            }

            AddDeleteOperation(item, restriction);
        }

        private void AddGetOperation(OpenApiPathItem item, NavigationPropertyRestriction restriction)
        {
            ReadRestrictionsType navPropReadRestriction = restriction?.ReadRestrictions ??
                Context.Model.GetRecord<ReadRestrictionsType>(NavigationProperty);
            ReadRestrictionsType entityReadRestriction = Context.Model.GetRecord<ReadRestrictionsType>(_navPropEntityType);
            bool isReadableDefault = navPropReadRestriction == null && entityReadRestriction == null;

            if (isReadableDefault)
            {
                AddOperation(item, OperationType.Get);
                return;
            }

            if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // TODO: $ref also supports Get ?
                if (LastSegmentIsKeySegment)
                {
                    if ((navPropReadRestriction?.ReadByKeyRestrictions?.IsReadable ?? true) &&
                        (entityReadRestriction?.IsReadable ?? true))
                    {
                        AddOperation(item, OperationType.Get);
                    }
                }
                else
                {
                    if ((navPropReadRestriction?.IsReadable ?? true) &&
                        (entityReadRestriction?.IsReadable ?? true))
                    {
                        AddOperation(item, OperationType.Get);
                    }
                }
            }
            else
            {
                Debug.Assert(LastSegmentIsKeySegment == false);
                if ((navPropReadRestriction?.IsReadable ?? true) &&
                   (entityReadRestriction?.IsReadable ?? true))
                {
                    AddOperation(item, OperationType.Get);
                }
            }
        }

        private void AddDeleteOperation(OpenApiPathItem item, NavigationPropertyRestriction restriction)
        {
            Debug.Assert(!LastSegmentIsRefSegment);

            if (!NavigationProperty.ContainsTarget)
            {
                return;
            }

            DeleteRestrictionsType navPropDeleteRestriction = restriction?.DeleteRestrictions ??
                Context.Model.GetRecord<DeleteRestrictionsType>(NavigationProperty);
            DeleteRestrictionsType entityDeleteRestriction = Context.Model.GetRecord<DeleteRestrictionsType>(_navPropEntityType);
            bool isDeletableDefault = navPropDeleteRestriction == null && entityDeleteRestriction == null;

            if (isDeletableDefault ||
               ((entityDeleteRestriction?.IsDeletable ?? true) &&
               (navPropDeleteRestriction?.IsDeletable ?? true)))
            {
                if (NavigationProperty.TargetMultiplicity() != EdmMultiplicity.Many || LastSegmentIsKeySegment)
                {
                    AddOperation(item, OperationType.Delete);
                }
                return;
            }
        }

        private void AddUpdateOperation(OpenApiPathItem item, NavigationPropertyRestriction restriction, UpdateRestrictionsType updateRestrictionsType = null)
        {
            UpdateRestrictionsType update = restriction?.UpdateRestrictions ?? updateRestrictionsType;
            if (update == null || update.IsUpdatable)
            {
                if (update != null && update.IsUpdateMethodPut)
                {
                    AddOperation(item, OperationType.Put);
                }
                else
                {
                    AddOperation(item, OperationType.Patch);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            LastSegmentIsKeySegment = path.LastSegment.Kind == ODataSegmentKind.Key;
            LastSegmentIsRefSegment = path.LastSegment.Kind == ODataSegmentKind.Ref;
            NavigationProperty = path.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;
            _navPropEntityType = NavigationProperty.ToEntityType();
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            if (!Context.Settings.ShowMsDosGroupPath)
            {
                return;
            }

            IList<ODataPath> samePaths = new List<ODataPath>();
            foreach (var path in Context.AllPaths.Where(p => p.Kind == ODataPathKind.NavigationProperty && p != Path))
            {
                bool lastIsKeySegment = path.LastSegment is ODataKeySegment;
                if (LastSegmentIsKeySegment != lastIsKeySegment)
                {
                    continue;
                }

                if (path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment &&
                    NavigationSource != navigationSourceSegment.NavigationSource)
                {
                    continue;
                }

				if (path.LastSegment is not ODataNavigationPropertySegment npSegment)
				{
					npSegment = path.Segments[path.Count - 2] as ODataNavigationPropertySegment;
				}
				if (NavigationProperty != npSegment.NavigationProperty)
                {
                    continue;
                }

                samePaths.Add(path);
            }

            if (samePaths.Any())
            {
                OpenApiArray array = new();
                OpenApiConvertSettings settings = Context.Settings.Clone();
                settings.EnableKeyAsSegment = Context.KeyAsSegment;
                foreach (var p in samePaths)
                {
                    array.Add(new OpenApiString(p.GetPathItemName(settings)));
                }

                item.Extensions.Add(Constants.xMsDosGroupPath, array);   
            }

            base.SetExtensions(item);
            item.Extensions.AddCustomAttributesToExtensions(Context, NavigationProperty);
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the {NavigationProperty.Name} property of the {NavigationProperty.DeclaringType.FullTypeName()} entity.";
        }
    }
}
