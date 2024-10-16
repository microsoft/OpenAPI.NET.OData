// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets a bool value indicating whether the second last segment in a $ref path is a key segment
        /// </summary>
        protected bool SecondLastSegmentIsKeySegment { get; private set; }

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
            SecondLastSegmentIsKeySegment = Path.Segments.Reverse().Skip(1).Take(1).Single().Kind == ODataSegmentKind.Key;
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
            string name = EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context);
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

        internal string GetOperationId(string prefix = null)
        {            
            return EdmModelHelper.GenerateNavigationPropertyPathOperationId(Path, Context, prefix);
        }               

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context.Settings.ShowExternalDocs)
            {
                var externalDocs = Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) ??
                    Context.Model.GetLinkRecord(NavigationProperty, CustomLinkRel);

                if (externalDocs != null)
                {
                    operation.ExternalDocs = new OpenApiExternalDocs()
                    {
                        Description = CoreConstants.ExternalDocsDescription,
                        Url = externalDocs.Href
                    };
                }
            }
        }

        /// <summary>
        /// Retrieves the CRUD restrictions annotations for the navigation property
        /// in context, given a capability annotation term.
        /// </summary>
        /// <param name="annotationTerm">The fully qualified restriction annotation term.</param>
        /// <returns>The restriction annotation, or null if not available.</returns>
        protected IRecord GetRestrictionAnnotation(string annotationTerm)
        {
            switch (annotationTerm)
            {
                case CapabilitiesConstants.ReadRestrictions:
                    var readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
                    readRestrictions?.MergePropertiesIfNull(Restriction?.ReadRestrictions);
                    readRestrictions ??= Restriction?.ReadRestrictions;

                    var navPropReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(NavigationProperty, CapabilitiesConstants.ReadRestrictions);
                    readRestrictions?.MergePropertiesIfNull(navPropReadRestrictions);
                    readRestrictions ??= navPropReadRestrictions;

                    return readRestrictions;
                case CapabilitiesConstants.UpdateRestrictions:
                    var updateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(TargetPath, CapabilitiesConstants.UpdateRestrictions);
                    updateRestrictions?.MergePropertiesIfNull(Restriction?.UpdateRestrictions);
                    updateRestrictions ??= Restriction?.UpdateRestrictions;

                    var navPropUpdateRestrictions = Context.Model.GetRecord<UpdateRestrictionsType>(NavigationProperty, CapabilitiesConstants.UpdateRestrictions);
                    updateRestrictions?.MergePropertiesIfNull(navPropUpdateRestrictions);
                    updateRestrictions ??= navPropUpdateRestrictions;

                    return updateRestrictions;
                case CapabilitiesConstants.InsertRestrictions:
                    var insertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(TargetPath, CapabilitiesConstants.InsertRestrictions);
                    insertRestrictions?.MergePropertiesIfNull(Restriction?.InsertRestrictions);
                    insertRestrictions ??= Restriction?.InsertRestrictions;

                    var navPropInsertRestrictions = Context.Model.GetRecord<InsertRestrictionsType>(NavigationProperty, CapabilitiesConstants.InsertRestrictions);
                    insertRestrictions?.MergePropertiesIfNull(navPropInsertRestrictions);
                    insertRestrictions ??= navPropInsertRestrictions;

                    return insertRestrictions;
                case CapabilitiesConstants.DeleteRestrictions:
                    var deleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(TargetPath, CapabilitiesConstants.DeleteRestrictions);
                    deleteRestrictions?.MergePropertiesIfNull(Restriction?.DeleteRestrictions);
                    deleteRestrictions ??= Restriction?.DeleteRestrictions;

                    var navPropDeleteRestrictions = Context.Model.GetRecord<DeleteRestrictionsType>(NavigationProperty, CapabilitiesConstants.DeleteRestrictions);
                    deleteRestrictions?.MergePropertiesIfNull(navPropDeleteRestrictions);
                    deleteRestrictions ??= navPropDeleteRestrictions;

                    return deleteRestrictions;
                default:
                    return null;

            }
        }

        protected IDictionary<string, OpenApiMediaType> GetContent(OpenApiSchema schema = null, IEnumerable<string> mediaTypes = null)
        {
            schema ??= GetOpenApiSchema();
            var content = new Dictionary<string, OpenApiMediaType>();

            if (mediaTypes != null)
            {
                foreach (string mediaType in mediaTypes)
                {
                    content.Add(mediaType, new OpenApiMediaType
                    {
                        Schema = schema
                    });
                }
            }
            else
            {
                // Default content type
                content.Add(Constants.ApplicationJsonMediaType, new OpenApiMediaType
                {
                    Schema = schema
                });
            };

            return content;
        }

        protected OpenApiSchema GetOpenApiSchema()
        {
            return new OpenApiSchema
            {
                UnresolvedReference = true,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = NavigationProperty.ToEntityType().FullName()
                }
            };
        }
    }
}
