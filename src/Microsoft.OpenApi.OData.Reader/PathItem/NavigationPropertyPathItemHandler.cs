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
using System.Text.Json.Nodes;
using System.Net.Http;
using Microsoft.OpenApi.Interfaces;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for a single <see cref="IEdmNavigationProperty"/>.
    /// </summary>
    internal class NavigationPropertyPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public NavigationPropertyPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.NavigationProperty;

        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        public IEdmNavigationProperty? NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource? NavigationSource { get; private set; }

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
        private IEdmEntityType? _navPropEntityType;

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            IEdmVocabularyAnnotatable? target = NavigationSource switch {
                IEdmEntitySet es => es,
                IEdmSingleton singleton => singleton,
                _ => null
            };

            var targetPathRestrictionType = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<NavigationRestrictionsType>(TargetPath, CapabilitiesConstants.NavigationRestrictions);
            var navSourceRestrictionType = target is null ? null : Context?.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
            var navPropRestrictionType = NavigationProperty is null ? null : Context?.Model.GetRecord<NavigationRestrictionsType>(NavigationProperty, CapabilitiesConstants.NavigationRestrictions);           
           
            var restriction = targetPathRestrictionType?.RestrictedProperties?.FirstOrDefault() 
                ?? (Path is null ? null : navSourceRestrictionType?.RestrictedProperties?.FirstOrDefault(r => r.NavigationProperty == Path.NavigationPropertyPath()))
                ?? navPropRestrictionType?.RestrictedProperties?.FirstOrDefault();

            // Check whether the navigation property should be part of the path
            if (!EdmModelHelper.NavigationRestrictionsAllowsNavigability(navSourceRestrictionType, restriction) ||
                !EdmModelHelper.NavigationRestrictionsAllowsNavigability(navPropRestrictionType, restriction))
            {
                return;
            }

            AddGetOperation(item, restriction);

            // Update restrictions
            var navPropUpdateRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            navPropUpdateRestrictions?.MergePropertiesIfNull(restriction?.UpdateRestrictions);
            navPropUpdateRestrictions ??= restriction?.UpdateRestrictions;
            if (NavigationProperty is not null && Context is not null)
            {
                var updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(NavigationProperty);
                navPropUpdateRestrictions?.MergePropertiesIfNull(updateRestrictions);
                navPropUpdateRestrictions ??= updateRestrictions;
            }

            // Insert restrictions
            var navPropInsertRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            navPropInsertRestrictions?.MergePropertiesIfNull(restriction?.InsertRestrictions);
            navPropInsertRestrictions ??= restriction?.InsertRestrictions;
            if (NavigationProperty is not null && Context is not null)
            {
                var insertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(NavigationProperty);
                navPropInsertRestrictions?.MergePropertiesIfNull(insertRestrictions);
                navPropInsertRestrictions ??= insertRestrictions;
            }

            // Entity insert restrictions
            var entityUpdateRestrictions = _navPropEntityType is null ? null : Context?.Model.GetRecord<UpdateRestrictionsType>(_navPropEntityType);

            // containment: (Post - Collection | Patch/Put - Single)           
            if (NavigationProperty?.ContainsTarget ?? false)
            {
                var updateRestrictionType = navPropUpdateRestrictions ?? entityUpdateRestrictions;
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
                        var entityInsertRestrictions = _navPropEntityType is null ? null : Context?.Model.GetRecord<InsertRestrictionsType>(_navPropEntityType);
                        bool isInsertableDefault = navPropInsertRestrictions == null && entityInsertRestrictions == null;

                        if (isInsertableDefault ||
                           ((entityInsertRestrictions?.IsInsertable ?? true) &&
                           (navPropInsertRestrictions?.IsInsertable ?? true)))
                        {
                            AddOperation(item, HttpMethod.Post);
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
                            AddOperation(item, HttpMethod.Post);
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

        private void AddGetOperation(OpenApiPathItem item, NavigationPropertyRestriction? restriction)
        {
            var navPropReadRestriction = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            navPropReadRestriction?.MergePropertiesIfNull(restriction?.ReadRestrictions);
            navPropReadRestriction ??= restriction?.ReadRestrictions;

            if (NavigationProperty is not null && Context is not null)
            {
                var readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(NavigationProperty);
                navPropReadRestriction?.MergePropertiesIfNull(readRestrictions);
                navPropReadRestriction ??= readRestrictions;
            }

            var entityReadRestriction = _navPropEntityType is null ? null : Context?.Model.GetRecord<ReadRestrictionsType>(_navPropEntityType);
            bool isReadableDefault = navPropReadRestriction == null && entityReadRestriction == null;
            if (isReadableDefault)
            {
                AddOperation(item, HttpMethod.Get);
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
                        AddOperation(item, HttpMethod.Get);
                    }
                }
                else
                {
                    if ((navPropReadRestriction?.IsReadable ?? true) &&
                        (entityReadRestriction?.IsReadable ?? true))
                    {
                        AddOperation(item, HttpMethod.Get);
                    }
                }
            }
            else
            {
                Debug.Assert(!LastSegmentIsKeySegment);
                if ((navPropReadRestriction?.IsReadable ?? true) &&
                   (entityReadRestriction?.IsReadable ?? true))
                {
                    AddOperation(item, HttpMethod.Get);
                }
            }
        }

        private void AddDeleteOperation(OpenApiPathItem item, NavigationPropertyRestriction? restriction)
        {
            Debug.Assert(!LastSegmentIsRefSegment);

            var navPropDeleteRestriction = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            navPropDeleteRestriction?.MergePropertiesIfNull(restriction?.DeleteRestrictions);
            navPropDeleteRestriction ??= restriction?.DeleteRestrictions;
            if (NavigationProperty is not null && Context is not null)
            {
                var insertRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(NavigationProperty);
                navPropDeleteRestriction?.MergePropertiesIfNull(insertRestrictions);
                navPropDeleteRestriction ??= insertRestrictions;
            }

            if (!(NavigationProperty.TargetMultiplicity() != EdmMultiplicity.Many || LastSegmentIsKeySegment))
                return;

            var entityDeleteRestriction = _navPropEntityType is null ? null : Context?.Model.GetRecord<DeleteRestrictionsType>(_navPropEntityType);
            bool isDeletable = 
                (navPropDeleteRestriction == null && entityDeleteRestriction == null) ||
                ((entityDeleteRestriction?.IsDeletable ?? true) &&
                (navPropDeleteRestriction?.IsDeletable ?? true));

            if ((NavigationProperty?.ContainsTarget ?? false) && isDeletable)
            {
                AddOperation(item, HttpMethod.Delete);
            }
            else if (navPropDeleteRestriction?.Deletable ?? false)
            {
                // Add delete operation for non-contained nav. props only if explicitly set to true via annotation
                // Note: Use Deletable and NOT IsDeletable
                AddOperation(item, HttpMethod.Delete);
            }

            return;
        }

        private void AddUpdateOperation(OpenApiPathItem item, UpdateRestrictionsType? updateRestrictionsType)
        {
            if (updateRestrictionsType?.IsUpdatable ?? true)
            {
                if (updateRestrictionsType?.IsUpdateMethodPutAndPatch == true)
                {
                    AddOperation(item, HttpMethod.Put);
                    AddOperation(item, HttpMethod.Patch);
                }
                else if (updateRestrictionsType?.IsUpdateMethodPut == true)
                {
                    AddOperation(item, HttpMethod.Put);
                }
                else
                {
                    AddOperation(item, HttpMethod.Patch);
                }
            }

        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: {} source})
                NavigationSource = source;

            LastSegmentIsKeySegment = path.LastSegment?.Kind == ODataSegmentKind.Key;
            LastSegmentIsRefSegment = path.LastSegment?.Kind == ODataSegmentKind.Ref;
            NavigationProperty = path.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;
            _navPropEntityType = NavigationProperty.ToEntityType();
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiPathItem item)
        {
            if (Context is null || !Context.Settings.ShowMsDosGroupPath)
            {
                return;
            }

            var samePaths = new List<ODataPath>();
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

                var npSegment = path.LastSegment is ODataNavigationPropertySegment npS ? 
                                npS :
                                path.Segments[path.Count - 2] is ODataNavigationPropertySegment npSegmentFallback ?
                                npSegmentFallback :
                                null;
				if (NavigationProperty != npSegment?.NavigationProperty)
                {
                    continue;
                }

                samePaths.Add(path);
            }

            if (samePaths.Any())
            {
                JsonArray array = new();
                OpenApiConvertSettings settings = Context.Settings.Clone();
                settings.EnableKeyAsSegment = Context.KeyAsSegment;
                foreach (var p in samePaths)
                {
                    array.Add(p.GetPathItemName(settings));
                }

                item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                item.Extensions.Add(Constants.xMsDosGroupPath, new OpenApiAny(array));   
            }

            base.SetExtensions(item);
            if (NavigationProperty is not null)
            {
                item.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                item.Extensions.AddCustomAttributesToExtensions(Context, NavigationProperty);
            }
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the {NavigationProperty?.Name} property of the {NavigationProperty?.DeclaringType.FullTypeName()} entity.";
        }
    }
}
