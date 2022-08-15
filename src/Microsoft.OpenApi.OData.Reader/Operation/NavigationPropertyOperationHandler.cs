// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of <see cref="IEdmNavigationProperty"/>.
    /// </summary>
    internal abstract class NavigationPropertyOperationHandler : OperationHandler
    {
        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        protected IEdmNavigationProperty NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        protected IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the navigation restriction.
        /// </summary>
        protected NavigationPropertyRestriction Restriction { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a key segment.
        /// </summary>
        protected bool LastSegmentIsKeySegment { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a $ref segment.
        /// </summary>
        protected bool LastSegmentIsRefSegment { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            base.Initialize(context, path);

            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            NavigationSource = navigationSourceSegment.NavigationSource;

            LastSegmentIsKeySegment = path.LastSegment is ODataKeySegment;
            LastSegmentIsRefSegment = path.LastSegment is ODataRefSegment;
            NavigationProperty = path.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;

            IEdmEntitySet entitySet = NavigationSource as IEdmEntitySet;
            IEdmSingleton singleton = NavigationSource as IEdmSingleton;

            NavigationRestrictionsType navigation;
            if (entitySet != null)
            {
                navigation = Context.Model.GetRecord<NavigationRestrictionsType>(entitySet, CapabilitiesConstants.NavigationRestrictions);
            }
            else
            {
                navigation = Context.Model.GetRecord<NavigationRestrictionsType>(singleton, CapabilitiesConstants.NavigationRestrictions);
            }

            Restriction = navigation?.RestrictedProperties?.FirstOrDefault(r => r.NavigationProperty != null && r.NavigationProperty == Path.NavigationPropertyPath())
                    ?? Context.Model.GetRecord<NavigationRestrictionsType>(NavigationProperty, CapabilitiesConstants.NavigationRestrictions)?.RestrictedProperties?.FirstOrDefault();
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            IList<string> items = new List<string>
            {
                NavigationSource.Name
            };

            foreach (var segment in Path.Segments.Skip(1).OfType<ODataNavigationPropertySegment>())
            {
                if (segment.NavigationProperty == NavigationProperty)
                {
                    items.Add(NavigationProperty.ToEntityType().Name);
                    break;
                }
                else
                {
                    if (items.Count >= Context.Settings.TagDepth - 1)
                    {
                        items.Add(segment.NavigationProperty.ToEntityType().Name);
                        break;
                    }
                    else
                    {
                        items.Add(segment.NavigationProperty.Name);
                    }
                }
            }

            string name = string.Join(".", items);
            OpenApiTag tag = new()
            {
                Name = name
            };
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));
            operation.Tags.Add(tag);

            Context.AppendTag(tag);

            base.SetTags(operation);
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            operation.Extensions.Add(Constants.xMsDosOperationType, new OpenApiString("operation"));

            base.SetExtensions(operation);
        }

        protected string GetOperationId(string prefix = null)
        {
            IList<string> items = new List<string>
            {
                NavigationSource.Name
            };

            var lastpath = Path.Segments.Last(c => c is ODataNavigationPropertySegment);
            foreach (var segment in Path.Segments.Skip(1).OfType<ODataNavigationPropertySegment>())
            {
                if (segment == lastpath)
                {
                    if (prefix != null)
                    {
                        items.Add(prefix + Utils.UpperFirstChar(segment.NavigationProperty.Name));
                    }
                    else
                    {
                        items.Add(Utils.UpperFirstChar(segment.NavigationProperty.Name));
                    }

                    break;
                }
                else
                {
                    items.Add(segment.NavigationProperty.Name);
                }
            }

            return string.Join(".", items);
        }

        /// <summary>
        /// Retrieves the CRUD restrictions annotations for the navigation property
        /// in context, given a capability annotation term.
        /// </summary>
        /// <param name="annotationTerm">The fully qualified restriction annotation term.</param>
        /// <returns>The restriction annotation, or null if not available.</returns>
        protected IRecord GetRestrictionAnnotation(string annotationTerm)
        {
            return annotationTerm switch
            {
                CapabilitiesConstants.ReadRestrictions => Restriction?.ReadRestrictions ??
                                        Context.Model.GetRecord<ReadRestrictionsType>(NavigationProperty, CapabilitiesConstants.ReadRestrictions),
                CapabilitiesConstants.UpdateRestrictions => Restriction?.UpdateRestrictions ??
                                        Context.Model.GetRecord<UpdateRestrictionsType>(NavigationProperty, CapabilitiesConstants.UpdateRestrictions),
                CapabilitiesConstants.InsertRestrictions => Restriction?.InsertRestrictions ??
                                        Context.Model.GetRecord<InsertRestrictionsType>(NavigationProperty, CapabilitiesConstants.InsertRestrictions),
                CapabilitiesConstants.DeleteRestrictions => Restriction?.DeleteRestrictions ??
                                        Context.Model.GetRecord<DeleteRestrictionsType>(NavigationProperty, CapabilitiesConstants.DeleteRestrictions),
                _ => null,
            };
        }
    }
}
