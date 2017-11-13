//---------------------------------------------------------------------
// <copyright file="EdmOpenApiDocumentGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// A class used to covert Edm model to <see cref="OpenApiDocument"/>
    /// </summary>
    internal class OpenApiDocumentGenerator
    {
        private OpenApiComponentsGenerator _componentsGenerator;
        private OpenApiPathsGenerator _pathsGenerator;

        /// <summary>
        /// The Edm model.
        /// </summary>
        protected IEdmModel Model { get; }

        /// <summary>
        /// The Open Api document external configuration action.
        /// </summary>
        private Action<OpenApiDocument> _configure;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public OpenApiDocumentGenerator(IEdmModel model)
            : this(model, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmOpenApiDocumentGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        public OpenApiDocumentGenerator(IEdmModel model, Action<OpenApiDocument> configure)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            _configure = configure;
            _componentsGenerator = new OpenApiComponentsGenerator(model);
            _pathsGenerator = new OpenApiPathsGenerator(model);
        }

        /// <summary>
        /// Generate Open Api document.
        /// </summary>
        /// <returns>The <see cref="OpenApiDocument"/> object.</returns>
        public OpenApiDocument Generate()
        {
            OpenApiDocument document = new OpenApiDocument
            {
                SpecVersion = new Version(3, 0, 0),

                Info = CreateInfo(),

                Servers = CreateServers(),

                Paths = CreatePaths(),

                Components = CreateComponents(),

                Tags = CreateTags()
            };

            _configure?.Invoke(document);

            return document;
        }

        /// <summary>
        /// Create <see cref="OpenApiInfo"/> object.
        /// </summary>
        /// <returns>The info object.</returns>
        private OpenApiInfo CreateInfo()
        {
            // The value of info is an Info Object,
            // It contains the fields title and version, and optionally the field description.
            return new OpenApiInfo
            {
                // The value of title is the value of the unqualified annotation Core.Description
                // of the main schema or the entity container of the OData service.
                // If no Core.Description is present, a default title has to be provided as this is a required OpenAPI field.
                Title = "OData Service for namespace " + Model.DeclaredNamespaces.FirstOrDefault(),

                // The value of version is the value of the annotation Core.SchemaVersion(see[OData - VocCore]) of the main schema.
                // If no Core.SchemaVersion is present, a default version has to be provided as this is a required OpenAPI field.
                Version = "1.0.0",

                // The value of description is the value of the annotation Core.LongDescription
                // of the main schema or the entity container.
                // While this field is optional, it prominently appears in OpenAPI exploration tools,
                // so a default description should be provided if no Core.LongDescription annotation is present.
                // Description = "This OData service is located at " + Settings.BaseUri?.OriginalString
            };
        }

        /// <summary>
        /// Create the collection of <see cref="OpenApiServer"/> object.
        /// </summary>
        /// <returns>The servers object.</returns>
        private IList<OpenApiServer> CreateServers()
        {
            // The value of servers is an array of Server Objects.
            // It contains one object with a field url.
            // The value of url is a string cotaining the service root URL without the trailing forward slash.
            return new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = string.Empty
                }
            };
        }

        /// <summary>
        /// Create the <see cref="OpenApiPaths"/> object.
        /// </summary>
        /// <returns>The paths object.</returns>
        private OpenApiPaths CreatePaths()
        {
            return _pathsGenerator.Generate();
        }

        // <summary>
        /// Create the <see cref="OpenApiComponents"/> object.
        /// </summary>
        /// <returns>The components object.</returns>
        private OpenApiComponents CreateComponents()
        {
            return _componentsGenerator.Generate();
        }

        // <summary>
        /// Create the collection of <see cref="OpenApiTag"/> object.
        /// </summary>
        /// <returns>The tag object.</returns>
        private IList<OpenApiTag> CreateTags()
        {
            // The value of tags is an array of Tag Objects.
            // For an OData service the natural groups are entity sets and singletons,
            // so the tags array contains one Tag Object per entity set and singleton in the entity container.
            IList<OpenApiTag> tags = new List<OpenApiTag>();
            if (Model.EntityContainer != null)
            {
                foreach (IEdmEntityContainerElement element in Model.EntityContainer.Elements)
                {
                    switch(element.ContainerElementKind)
                    {
                        case EdmContainerElementKind.EntitySet: // entity set
                            IEdmEntitySet entitySet = (IEdmEntitySet)element;
                            tags.Add(new OpenApiTag
                            {
                                Name = entitySet.Name,
                                Description = Model.GetDescription(entitySet)
                            });
                            break;

                        case EdmContainerElementKind.Singleton: // singleton
                            IEdmSingleton singleton = (IEdmSingleton)element;
                            tags.Add(new OpenApiTag
                            {
                                Name = singleton.Name,
                                Description = Model.GetDescription(singleton)
                            });
                            break;

                            // The tags array can contain additional Tag Objects for other logical groups,
                            // e.g. for action imports or function imports that are not associated with an entity set.
                        case EdmContainerElementKind.ActionImport: // Action Import
                            break;

                        case EdmContainerElementKind.FunctionImport: // Function Import
                            break;
                    }
                }
            }

            // Tags is optional.
            return tags.Any() ? tags : null;
        }
    }
}
