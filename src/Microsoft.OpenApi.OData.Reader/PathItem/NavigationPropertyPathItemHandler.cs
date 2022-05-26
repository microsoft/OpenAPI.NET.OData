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

        private IEdmEntityType _entityType;

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
                        UpdateRestrictionsType updateEntity = Context.Model.GetRecord<UpdateRestrictionsType>(_entityType);
                        if (updateEntity?.IsUpdatable ?? true)
                        {
                            AddUpdateOperation(item, restriction);
                        }
                    }
                    else
                    {
                        InsertRestrictionsType insert = restriction?.InsertRestrictions;
                        if (insert?.IsInsertable ?? true)
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
            ReadRestrictionsType read = restriction?.ReadRestrictions;
            if (read == null)
            {
                AddOperation(item, OperationType.Get);
                return;
            }

            if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                // TODO: $ref also supports Get ?
                if (LastSegmentIsKeySegment)
                {
                    if (read.ReadByKeyRestrictions != null && read.ReadByKeyRestrictions.Readable != null)
                    {
                        if (read.ReadByKeyRestrictions.Readable.Value)
                        {
                            AddOperation(item, OperationType.Get);
                        }
                    }
                    else
                    {
                        ReadRestrictionsType readEntity = Context.Model.GetRecord<ReadRestrictionsType>(_entityType);
                        if (readEntity?.IsReadable ?? true)
                        {
                            AddOperation(item, OperationType.Get);
                        }
                    }
                }
                else
                {
                    if (read.IsReadable)
                    {
                        AddOperation(item, OperationType.Get);
                    }
                }
            }
            else
            {
                Debug.Assert(LastSegmentIsKeySegment == false);
                if (read.IsReadable)
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

            DeleteRestrictionsType delete = restriction?.DeleteRestrictions;
            DeleteRestrictionsType deleteEntity = Context.Model.GetRecord<DeleteRestrictionsType>(_entityType);
            bool isDeletable = (delete?.IsDeletable ?? true) || (deleteEntity?.IsDeletable ?? true);

            if (isDeletable)
            {
                AddOperation(item, OperationType.Delete);
                return;
            }
        }

        private void AddUpdateOperation(OpenApiPathItem item, NavigationPropertyRestriction restriction)
        {
            UpdateRestrictionsType update = restriction?.UpdateRestrictions;
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
            _entityType = NavigationProperty.ToEntityType();
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
            item.Extensions.AddCustomAtributesToExtensions(Context, NavigationProperty);
        }
        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the {NavigationProperty.Name} property of the {NavigationProperty.DeclaringType.FullTypeName()} entity.";
        }
    }
}
