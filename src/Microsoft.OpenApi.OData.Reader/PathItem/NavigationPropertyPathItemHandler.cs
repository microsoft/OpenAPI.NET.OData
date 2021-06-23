// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
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
using System.Drawing;

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

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            IEdmEntitySet entitySet = NavigationSource as IEdmEntitySet;
            IEdmVocabularyAnnotatable target = entitySet;
            if (target == null)
            {
                target = NavigationSource as IEdmSingleton;
            }

            string navigationPropertyPath = String.Join("/",
                Path.Segments.Where(s => !(s is ODataKeySegment || s is ODataNavigationSourceSegment)).Select(e => e.Identifier));

            NavigationRestrictionsType navigation = Context.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
            NavigationPropertyRestriction restriction = navigation?.RestrictedProperties?.FirstOrDefault(r => r.NavigationProperty == navigationPropertyPath);

            // verify using individual first
            if (restriction != null && restriction.Navigability != null && restriction.Navigability.Value == NavigationType.None)
            {
                return;
            }

            if (restriction == null || restriction.Navigability == null)
            {
                // if the individual has not navigability setting, use the global navigability setting
                if (navigation != null && navigation.Navigability != null && navigation.Navigability.Value == NavigationType.None)
                {
                    // Default navigability for all navigation properties of the annotation target.
                    // Individual navigation properties can override this value via `RestrictedProperties/Navigability`.
                    return;
                }
            }

            // contaiment: Get / (Post - Collection | Patch - Single)
            // non-containment: Get
            AddGetOperation(item, restriction);

            if (NavigationProperty.ContainsTarget)
            {
                if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                {
                    if (LastSegmentIsKeySegment)
                    {
                        // Need to check this scenario is valid or not?
                        UpdateRestrictionsType update = restriction?.UpdateRestrictions;
                        if (update == null || update.IsUpdatable)
                        {
                            AddOperation(item, OperationType.Patch);
                        }
                    }
                    else
                    {
                        InsertRestrictionsType insert = restriction?.InsertRestrictions;
                        if (insert == null || insert.IsInsertable)
                        {
                            AddOperation(item, OperationType.Post);
                        }
                    }
                }
                else
                {
                    UpdateRestrictionsType update = restriction?.UpdateRestrictions;
                    if (update == null || update.IsUpdatable)
                    {
                        AddOperation(item, OperationType.Patch);
                    }
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
                        if (read.IsReadable)
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
            if (delete == null || delete.IsDeletable)
            {
                if (NavigationProperty.TargetMultiplicity() != EdmMultiplicity.Many || LastSegmentIsKeySegment)
                {
                    AddOperation(item, OperationType.Delete);
                }

                return;
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

                ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
                if (NavigationSource != navigationSourceSegment.NavigationSource)
                {
                    continue;
                }

                ODataNavigationPropertySegment npSegment = path.LastSegment as ODataNavigationPropertySegment;
                if (npSegment == null)
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
                OpenApiArray array = new OpenApiArray();
                OpenApiConvertSettings settings = Context.Settings.Clone();
                settings.EnableKeyAsSegment = Context.KeyAsSegment;
                foreach (var p in samePaths)
                {
                    array.Add(new OpenApiString(p.GetPathItemName(settings)));
                }

                item.Extensions.Add(Constants.xMsDosGroupPath, array);
            }
        }
    }
}
