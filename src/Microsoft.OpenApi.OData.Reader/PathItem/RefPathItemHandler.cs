// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Net.Http;

namespace Microsoft.OpenApi.OData.PathItem
{
    /// <summary>
    /// Create a <see cref="OpenApiPathItem"/> for a single $ref.
    /// </summary>
    internal class RefPathItemHandler : PathItemHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefPathItemHandler"/> class.
        /// </summary>
        /// <param name="document">The document to use for references lookup.</param>
        public RefPathItemHandler(OpenApiDocument document) : base(document)
        {
            
        }
        /// <inheritdoc/>
        protected override ODataPathKind HandleKind => ODataPathKind.Ref;

        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        public IEdmNavigationProperty? NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource? NavigationSource { get; private set; }

        /// <inheritdoc/>
        protected override void SetOperations(OpenApiPathItem item)
        {
            if (NavigationSource is not IEdmVocabularyAnnotatable target)
                throw new InvalidOperationException($"The navigation source {NavigationSource?.Name} is not a vocabulary annotatable.");

            string navigationPropertyPath = string.Join("/",
                Path?.Segments.Where(s => !(s is ODataKeySegment || s is ODataNavigationSourceSegment)).Select(e => e.Identifier) ?? []);
           
            var navigationRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<NavigationRestrictionsType>(TargetPath, CapabilitiesConstants.NavigationRestrictions);
            
            if (Context is not null && target is not null)
            {
                var sourceNavigationRestrictions = Context.Model.GetRecord<NavigationRestrictionsType>(target, CapabilitiesConstants.NavigationRestrictions);
                navigationRestrictions?.MergePropertiesIfNull(sourceNavigationRestrictions);
                navigationRestrictions ??= sourceNavigationRestrictions;
            }

            var restriction = navigationRestrictions?.RestrictedProperties?.FirstOrDefault(r => r.NavigationProperty == navigationPropertyPath);

            // verify using individual first
            if (restriction?.Navigability != null && restriction.Navigability.Value == NavigationType.None)
            {
                return;
            }

            // if the individual has not navigability setting, use the global navigability setting
            if (restriction?.Navigability == null 
                && navigationRestrictions != null 
                && navigationRestrictions.Navigability != null 
                && navigationRestrictions.Navigability.Value == NavigationType.None)
            {
                // Default navigability for all navigation properties of the annotation target.
                // Individual navigation properties can override this value via `RestrictedProperties/Navigability`.
                return;
            }

            // Create the ref
            if (NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                if (Path?.Segments.Reverse().Skip(1).First() is ODataKeySegment)
                {
                    // Collection-valued indexed: DELETE ~/entityset/{key}/collection-valued-Nav/{key}/$ref
                    AddDeleteOperation(item, restriction);
                }
                else
                {
                    AddReadOperation(item, restriction);
                    AddInsertOperation(item, restriction);
                    // Collection-valued: DELETE ~/entityset/{key}/collection-valued-Nav/$ref?$id='{navKey}'
                    AddDeleteOperation(item, restriction);
                }
            }
            else
            {
                AddReadOperation(item, restriction);
                AddUpdateOperation(item, restriction);
                AddDeleteOperation(item, restriction);
            }
        }

        private void AddDeleteOperation(OpenApiPathItem item, NavigationPropertyRestriction? restriction)
        {
            var deleteRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
            deleteRestrictions?.MergePropertiesIfNull(restriction?.DeleteRestrictions);
            deleteRestrictions ??= restriction?.DeleteRestrictions;
            if (deleteRestrictions?.IsDeletable ?? true)
            {
                AddOperation(item, HttpMethod.Delete);
            }
        }

        private void AddReadOperation(OpenApiPathItem item, NavigationPropertyRestriction? restriction)
        {
            var readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
            readRestrictions?.MergePropertiesIfNull(restriction?.ReadRestrictions);
            readRestrictions ??= restriction?.ReadRestrictions;
            if (readRestrictions?.IsReadable ?? true)
            {
                AddOperation(item, HttpMethod.Get);
            }
        }

        private void AddInsertOperation(OpenApiPathItem item, NavigationPropertyRestriction? restriction)
        {
            var insertRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
            insertRestrictions?.MergePropertiesIfNull(restriction?.InsertRestrictions);
            insertRestrictions ??= restriction?.InsertRestrictions;
            if (insertRestrictions?.IsInsertable ?? true)
            {
                AddOperation(item, HttpMethod.Post);
            }
        }

        private void AddUpdateOperation(OpenApiPathItem item, NavigationPropertyRestriction? restriction)
        {
            var updateRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
            updateRestrictions?.MergePropertiesIfNull(restriction?.UpdateRestrictions);
            updateRestrictions ??= restriction?.UpdateRestrictions;
            if (updateRestrictions?.IsUpdatable ?? true)
            {
                AddOperation(item, HttpMethod.Put);
            }
        }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            if (path.FirstSegment is ODataNavigationSourceSegment {NavigationSource: {} source})
            {
                NavigationSource = source;
            }

            NavigationProperty = path.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;
        }

        /// <inheritdoc/>
        protected override void SetBasicInfo(OpenApiPathItem pathItem)
        {
            base.SetBasicInfo(pathItem);
            pathItem.Description = $"Provides operations to manage the collection of {NavigationSource?.EntityType.Name ?? NavigationProperty?.Type.ShortQualifiedName()} entities.";
        }
    }
}
