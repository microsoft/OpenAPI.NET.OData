// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.OData.Operation
{
    /// <summary>
    /// Base class for operation of media entity.
    /// </summary>
    internal abstract class MediaEntityOperationalHandler : OperationHandler
    {
        /// <summary>
        /// Gets/Sets the NavigationSource segment
        /// </summary>
        protected ODataNavigationSourceSegment NavigationSourceSegment { get; private set; }
    
        /// <summary>
        /// Gets/Sets flag indicating whether path is navigation property path
        /// </summary>
        protected bool IsNavigationPropertyPath { get; private set; }

        protected bool LastSegmentIsStreamPropertySegment { get; private set; }

        protected bool LastSegmentIsStreamContentSegment { get; private set; }

        /// <summary>
        /// Gets a bool value indicating whether the last segment is a key segment.
        /// </summary>
        protected bool LastSegmentIsKeySegment { get; private set; }

        /// <summary>
        /// Gets the media entity property.
        /// </summary>
        protected IEdmProperty Property { get; private set; }

        /// <summary>
        /// Gets the navigation property.
        /// </summary>
        protected IEdmNavigationProperty NavigationProperty { get; private set; }

        /// <inheritdoc/>
        protected override void Initialize(ODataContext context, ODataPath path)
        {
            // The first segment will either be an EntitySet navigation source or a Singleton navigation source
            NavigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;

            // Check whether path is a navigation property path
            IsNavigationPropertyPath = Path.Segments.Contains(
                Path.Segments.Where(segment => segment is ODataNavigationPropertySegment).FirstOrDefault());

            LastSegmentIsStreamPropertySegment = Path.LastSegment.Kind == ODataSegmentKind.StreamProperty;

            LastSegmentIsStreamContentSegment = Path.LastSegment.Kind == ODataSegmentKind.StreamContent;
            
            LastSegmentIsKeySegment = path.LastSegment is ODataKeySegment;

            (_, Property) = GetStreamElements();

            if (IsNavigationPropertyPath)
            {
                NavigationProperty = path.OfType<ODataNavigationPropertySegment>().Last().NavigationProperty;
            }

            base.Initialize(context, path);
        }

        /// <inheritdoc/>
        protected override void SetTags(OpenApiOperation operation)
        {

            string tagIdentifier = IsNavigationPropertyPath 
                ? EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context)
                : NavigationSourceSegment.Identifier + "." + NavigationSourceSegment.EntityType.Name;

            OpenApiTag tag = new()
            {
                Name = tagIdentifier
            };

            // Use an extension for TOC (Table of Content)
            tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));

            operation.Tags.Add(tag);

            Context.AppendTag(tag);
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
                NavigationSourceSegment.Identifier
            };

            ODataSegment lastSegment = Path.Segments.Last(c => c is ODataStreamContentSegment || c is ODataStreamPropertySegment);
            foreach (ODataSegment segment in Path.Segments.Skip(1))
            {
                if (segment == lastSegment)
                {
                    if (!IsNavigationPropertyPath)
                    {
                        string typeName = NavigationSourceSegment.EntityType.Name;
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
            // Fetch the respective AcceptableMediaTypes
            (_, var property) = GetStreamElements();
            IEnumerable<string> mediaTypes = null;
            if (property != null)
            {
                mediaTypes = Context.Model.GetCollection(property,
                    CoreConstants.AcceptableMediaTypes);
            }

            OpenApiSchema schema = new()
            {
                Type = "string",
                Format = "binary"
            };

            var content = new Dictionary<string, OpenApiMediaType>();
            if (mediaTypes != null)
            {
                foreach (string item in mediaTypes)
                {
                    content.Add(item, new OpenApiMediaType
                    {
                        Schema = schema
                    });
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
        /// Gets the stream property and entity type declaring the stream property.
        /// </summary>
        /// <returns>The stream property and entity type declaring the stream property.</returns>
        protected (IEdmEntityType entityType, IEdmProperty property) GetStreamElements()
        {
            // Only ODataStreamPropertySegment is annotatable
            if (!LastSegmentIsStreamPropertySegment && !LastSegmentIsStreamContentSegment) return (null, null);

            // Retrieve the entity type of the segment before the stream property segment
            var entityType = LastSegmentIsStreamContentSegment
                ? Path.Segments.ElementAtOrDefault(Path.Segments.Count - 3).EntityType
                : Path.Segments.ElementAtOrDefault(Path.Segments.Count - 2).EntityType;

            // The stream property can either be a structural type or a navigation property type
            ODataSegment lastSegmentProp = LastSegmentIsStreamContentSegment
                ? Path.Segments.Reverse().Skip(1).FirstOrDefault()
                : Path.Segments.LastOrDefault(c => c is ODataStreamPropertySegment);
            IEdmProperty property = GetStructuralProperty(entityType, lastSegmentProp.Identifier);
            if (property == null)
            {
                property = GetNavigationProperty(entityType, lastSegmentProp.Identifier);
            }

            return (entityType, property);
        }

        private IEdmStructuralProperty GetStructuralProperty(IEdmEntityType entityType, string identifier)
        {
            return entityType.StructuralProperties().FirstOrDefault(x => x.Name.Equals(identifier));
        }

        private IEdmNavigationProperty GetNavigationProperty(IEdmEntityType entityType, string identifier)
        {
            return entityType.DeclaredNavigationProperties().FirstOrDefault(x => x.Name.Equals(identifier));
        }

        /// <inheritdoc/>
        protected override void SetExternalDocs(OpenApiOperation operation)
        {
            if (Context.Settings.ShowExternalDocs && Property != null)
            {
                var externalDocs = Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) ??
                    Context.Model.GetLinkRecord(Property, CustomLinkRel);

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
    }
}
