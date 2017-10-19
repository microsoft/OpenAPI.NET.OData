//---------------------------------------------------------------------
// <copyright file="EdmOpenApiDocumentGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// An class used to covert Edm model to <see cref="OpenApiDocument"/>
    /// </summary>
    internal class EdmOpenApiDocumentGenerator : EdmOpenApiGenerator
    {
        private OpenApiDocument _openApiDoc;
        private EdmOpenApiComponentsGenerator _componentsGenerator;
        private EdmOpenApiPathsGenerator _pathsGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmOpenApiDocumentGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        public EdmOpenApiDocumentGenerator(IEdmModel model, OpenApiWriterSettings settings)
            : base(model, settings)
        {
            _componentsGenerator = new EdmOpenApiComponentsGenerator(model, settings);
            _pathsGenerator = new EdmOpenApiPathsGenerator(model, settings);
        }

        /// <summary>
        /// Generate Open Api document.
        /// </summary>
        /// <returns>The <see cref="OpenApiDocument"/> object.</returns>
        public virtual OpenApiDocument Generate()
        {
            if (_openApiDoc == null)
            {
                _openApiDoc = new OpenApiDocument
                {
                    Info = CreateInfo(),

                    Servers = CreateServers(),

                    Paths = CreatePaths(),

                    Components = CreateComponents(),

                    Security = CreateSecurity(),

                    Tags = CreateTags()
                };
            }

            return _openApiDoc;
        }

        /// <summary>
        /// Create <see cref="OpenApiInfo"/> object.
        /// </summary>
        /// <returns>The info object.</returns>
        private OpenApiInfo CreateInfo()
        {
            return new OpenApiInfo
            {
                Title = "OData Service for namespace " + Model.DeclaredNamespaces.FirstOrDefault(),
                Version = Settings.Version,
                Description = "This OData service is located at " + Settings.BaseUri?.OriginalString
            };
        }

        /// <summary>
        /// Create the collection of <see cref="OpenApiServer"/> object.
        /// </summary>
        /// <returns>The servers object.</returns>
        private IList<OpenApiServer> CreateServers()
        {
            return new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = Settings.BaseUri
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
        /// Create the collection of <see cref="OpenApiSecurity"/> object.
        /// </summary>
        /// <returns>The security object.</returns>
        private IList<OpenApiSecurity> CreateSecurity()
        {
            return null;
        }

        // <summary>
        /// Create the collection of <see cref="OpenApiTag"/> object.
        /// </summary>
        /// <returns>The tag object.</returns>
        private IList<OpenApiTag> CreateTags()
        {
            IList<OpenApiTag> tags = new List<OpenApiTag>();
            if (Model.EntityContainer != null)
            {
                foreach (IEdmEntitySet entitySet in Model.EntityContainer.EntitySets())
                {
                    tags.Add(new OpenApiTag
                    {
                        Name = entitySet.Name
                    });
                }
            }

            return tags;
        }
    }
}
