// ------------------------------------------------------------
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
            target ??= NavigationSource as IEdmSingleton;                       

            NavigationRestrictionsType targetPathRestrictionType = Context.Model.GetRecord<NavigationRestrictionsType>(TargetPath, CapabilitiesConstants.NavigationRestrictions);
            NavigationRestrictionsType navSourceRestrictionType = Context.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
            NavigationRestrictionsType navPropRestrictionType = Context.Model.GetRecord<NavigationRestrictionsType>(NavigationProperty, CapabilitiesConstants.NavigationRestrictions);           
           
            NavigationPropertyRestriction restriction = targetPathRestrictionType?.RestrictedProperties?.FirstOrDefault() 
                ?? navSourceRestrictionType?.RestrictedProperties?.FirstOrDefault(r => r.NavigationProperty == Path.NavigationPropertyPath())
                ?? navPropRestrictionType?.RestrictedProperties?.FirstOrDefault();

            // Check whether the navigation property should be part of the path
            if (EdmModelHelper.NavigationRestrictionsAllowsNavigability(navSourceRestrictionType, restriction) == false ||
                EdmModelHelper.NavigationRestrictionsAllowsNavigability(navPropRestrictionType, restriction) == false)
            {
                return;
            }

            AddGetOperation(item, restriction);

            // Update restrictions
            UpdateRestrictionsType navPropUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            navPropUpdateRestrictions?.MergePropertiesIfNull(restriction?.UpdateRestrictions);
            navPropUpdateRestrictions ??= restriction?.UpdateRestrictions;
            UpdateRestrictionsType updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(NavigationProperty);
            navPropUpdateRestrictions?.MergePropertiesIfNull(updateRestrictions);
            navPropUpdateRestrictions ??= updateRestrictions;

            // Insert restrictions
            InsertRestrictionsType navPropInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            navPropInsertRestrictions?.MergePropertiesIfNull(restriction?.InsertRestrictions);
            navPropInsertRestrictions ??= restriction?.InsertRestrictions;
            InsertRestrictionsType insertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(NavigationProperty);
            navPropInsertRestrictions?.MergePropertiesIfNull(insertRestrictions);
            navPropInsertRestrictions ??= insertRestrictions;

            // Entity insert restrictions
            UpdateRestrictionsType entityUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(_navPropEntityType);

            // containment: (Post - Collection | Patch/Put - Single)           
            if (NavigationProperty.ContainsTarget)
            {
                UpdateRestrictionsType updateRestrictionType = navPropUpdateRestrictions ?? entityUpdateRestrictions;
                if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    if (LastSegmentIsKeySegment)
                    {

                        if ((entityUpdateRestrictions?.IsUpdatable ?? true) &&
                            (navPropUpdateRestrictions?.IsUpdatable ?? true))
                        {

                            AddUpdateOperation(item, updateRestrictionType);
                        }
                    }
                    else
                    {
                        InsertRestrictionsType entityInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(_navPropEntityType);
                        bool isInsertableDefault = navPropInsertRestrictions == null && entityInsertRestrictions == null;

                        if (isInsertableDefault ||
                           ((entityInsertRestrictions?.IsInsertable ?? true) &&
                           (navPropInsertRestrictions?.IsInsertable ?? true)))
                        {
                            AddOperation(item, OperationType.Post);
                        }
                    }
                }
                else
                {
                    AddUpdateOperation(item, updateRestrictionType);
                }
            }
            else
            {
                // non-containment: (Post - Collection | Patch/Put - Single) --> only if annotations are explicitly set to true
                // Note: Use Updatable and not IsUpdatable | Insertable and not IsInsertable
                if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    if (LastSegmentIsKeySegment)
                    {
                        if (navPropUpdateRestrictions?.Updatable ?? false)
                        {
                            AddUpdateOperation(item, navPropUpdateRestrictions);
                        }
                    }
                    else
                    {
                        if (navPropInsertRestrictions?.Insertable ?? false)
                        {
                            AddOperation(item, OperationType.Post);
                        }
                    }
                }
                else
                {
                    if (navPropUpdateRestrictions?.Updatable ?? false)
                    {
                        AddUpdateOperation(item, navPropUpdateRestrictions);
                    }             
                }
            }

            AddDeleteOperation(item, restriction);
        }

        private void AddGetOperation(OpenApiPathItem item, NavigationPropertyRestriction restriction)
        {
            ReadRestrictionsType navPropReadRestriction = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            navPropReadRestriction?.MergePropertiesIfNull(restriction?.ReadRestrictions);
            navPropReadRestriction ??= restriction?.ReadRestrictions;
            ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(NavigationProperty);
            navPropReadRestriction?.MergePropertiesIfNull(readRestrictions);
            navPropReadRestriction ??= readRestrictions;

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

            DeleteRestrictionsType navPropDeleteRestriction = Context.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            navPropDeleteRestriction?.MergePropertiesIfNull(restriction?.DeleteRestrictions);
            navPropDeleteRestriction ??= restriction?.DeleteRestrictions;
            DeleteRestrictionsType insertRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(NavigationProperty);
            navPropDeleteRestriction?.MergePropertiesIfNull(insertRestrictions);
            navPropDeleteRestriction ??= insertRestrictions;

            if (!(NavigationProperty.TargetMultiplicity() != EdmMultiplicity.Many || LastSegmentIsKeySegment))
                return;

            DeleteRestrictionsType entityDeleteRestriction = Context.Model.GetRecord<DeleteRestrictionsType>(_navPropEntityType);
            bool isDeletable = 
                (navPropDeleteRestriction == null && entityDeleteRestriction == null) ||
                ((entityDeleteRestriction?.IsDeletable ?? true) &&
                (navPropDeleteRestriction?.IsDeletable ?? true));

            if (NavigationProperty.ContainsTarget && isDeletable)
            {
                AddOperation(item, OperationType.Delete);
            }
            else if (navPropDeleteRestriction?.Deletable ?? false)
            {
                // Add delete operation for non-contained nav. props only if explicitly set to true via annotation
                // Note: Use Deletable and NOT IsDeletable
                AddOperation(item, OperationType.Delete);
            }

            return;
        }

        private void AddUpdateOperation(OpenApiPathItem item, UpdateRestrictionsType updateRestrictionsType)
        {
            if (updateRestrictionsType?.IsUpdatable ?? true)
            {
                if (updateRestrictionsType?.IsUpdateMethodPutAndPatch == true)
                {
                    AddOperation(item, OperationType.Put);
                    AddOperation(item, OperationType.Patch);
                }
                else if (updateRestrictionsType?.IsUpdateMethodPut == true)
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
