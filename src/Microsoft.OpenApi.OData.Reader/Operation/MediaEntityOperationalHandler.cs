// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of media entity.
    /// </summary>
    internal abstract class MediaEntityOperationalHandler : NavigationPropertyOperationHandler
    {
        /// <summary>
        /// Gets/sets the <see cref="IEdmEntitySet"/>.
        /// </summary>
        protected IEdmEntitySet EntitySet { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmSingleton"/>.
        /// </summary>
        protected IEdmSingleton Singleton { get; private set; }

        /// <summary>
        /// Gets/Sets flag indicating whether path is navigation property path
        /// </summary>
        protected bool IsNavigationPropertyPath { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            // The first segment will either be an EntitySet navigation source or a Singleton navigation source
            ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
            EntitySet = navigationSourceSegment.NavigationSource as IEdmEntitySet;

            if (EntitySet == null)
            {
                Singleton = navigationSourceSegment.NavigationSource as IEdmSingleton;
            }

            // Check whether path is a navigation property path
            IsNavigationPropertyPath = Path.Segments.Contains(
                Path.Segments.Where(segment => segment is ODataNavigationPropertySegment).FirstOrDefault());

            if (IsNavigationPropertyPath)
            {
                // Initialize navigation property paths from base
                base.Initialize(context, path);
            }
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {
            if (IsNavigationPropertyPath)
            {
                base.SetTags(operation);
            }
            else
            {
                string tagIdentifier = EntitySet.Name + "." + EntitySet.EntityType().Name;

                OpenApiTag tag = new OpenApiTag
                {
                    Name = tagIdentifier
                };

                // Use an extension for TOC (Table of Content)
                tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));

                operation.Tags.Add(tag);

                Context.AppendTag(tag);
            }
        }

        /// <inheritdoc/>
        protected override void SetExtensions(OpenApiOperation operation)
        {
            base.SetExtensions(operation);
        }

        /// <summary>
        /// Retrieves the operation Id for a media entity stream path.
        /// </summary>
        /// <param name="prefix">The http method identifier name.</param>
        /// <param name="identifier">The stream segment identifier name.</param>
        /// <returns></returns>
        protected string GetOperationId(string prefix, string identifier)
        {
            Utils.CheckArgumentNullOrEmpty(prefix, nameof(prefix));
            Utils.CheckArgumentNullOrEmpty(identifier, nameof(identifier));

            IList<string> items = new List<string>
            {
                EntitySet?.Name ?? Singleton.Name
            };

            ODataSegment lastSegment = Path.Segments.Last(c => c is ODataStreamContentSegment || c is ODataStreamPropertySegment);
            foreach (ODataSegment segment in Path.Segments.Skip(1))
            {
                if (segment == lastSegment)
                {
                    if (!IsNavigationPropertyPath)
                    {
                        string typeName = EntitySet?.EntityType().Name ?? Singleton.EntityType().Name;
                        items.Add(typeName);
                        items.Add(prefix + Utils.UpperFirstChar(identifier));
                    }
                    else
                    {
                        // Remove the last navigation property segment for navigation property paths,
                        // as this will be included within the prefixed name of the operation id
                        items.Remove(NavigationProperty.Name);
                        items.Add(prefix + Utils.UpperFirstChar(NavigationProperty.Name) + Utils.UpperFirstChar(identifier));
                    }
                    break;
                }
                else
                {
                    if (segment is ODataNavigationPropertySegment npSegment)
                    {
                        items.Add(npSegment.NavigationProperty.Name);
                    }
                }
            }

            return string.Join(".", items);
        }

        /// <summary>
        /// Gets a media entity content description.
        /// </summary>
        /// <returns>The entity content description.</returns>
        protected IDictionary<string, OpenApiMediaType> GetContentDescription()
        {
            var content = new Dictionary<string, OpenApiMediaType>();

            OpenApiSchema schema = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };

            // Fetch the respective AcceptableMediaTypes
            IEdmVocabularyAnnotatable annotatableElement = GetAnnotatableElement();
            IEnumerable<string> mediaTypes = null;
            if (annotatableElement != null)
            {
                mediaTypes = Context.Model.GetCollection(annotatableElement,
                    CapabilitiesConstants.AcceptableMediaTypes);
            }

            if (mediaTypes != null)
            {
                foreach (string item in mediaTypes)
                {
                    content.Add(item, null);
                }
            }
            else
            {
                // Default content type
                content.Add(Constants.ApplicationOctetStreamMediaType, new OpenApiMediaType
                {
                    Schema = schema
                });
            };

            return content;
        }

        /// <summary>
        /// Determines the annotatable element from the segments of a path.
        /// </summary>
        /// <returns>The annotable element.</returns>
        protected IEdmVocabularyAnnotatable GetAnnotatableElement()
        {
            IEdmEntityType entityType = EntitySet != null ? EntitySet.EntityType() : Singleton.EntityType();
            ODataSegment lastSegmentProp = Path.Segments.LastOrDefault(c => c is ODataStreamPropertySegment);

            if (lastSegmentProp == null)
            {
                int pathCount = Path.Segments.Count;

                // Retrieve the segment before the stream content segment
                lastSegmentProp = Path.Segments.ElementAtOrDefault(pathCount - 2);

                if (lastSegmentProp == null)
                {
                    return null;
                }
            }

            // Get the annotatable stream property
            // The stream property can either be a structural type or navigation type property
            IEdmProperty property = GetStructuralProperty(entityType, lastSegmentProp.Identifier);
            if (property == null)
            {
                property = GetNavigationProperty(entityType, lastSegmentProp.Identifier);
            }

            return property;
        }

        private IEdmStructuralProperty GetStructuralProperty(IEdmEntityType entityType, string identifier)
        {
            return entityType.DeclaredStructuralProperties().FirstOrDefault(x => x.Name.Equals(identifier));
        }

        private IEdmNavigationProperty GetNavigationProperty(IEdmEntityType entityType, string identifier)
        {
            return entityType.DeclaredNavigationProperties().FirstOrDefault(x => x.Name.Equals(identifier));
        }
    }
}
