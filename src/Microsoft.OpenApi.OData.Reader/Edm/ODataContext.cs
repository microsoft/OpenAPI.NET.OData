// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Annotations;
using Microsoft.OpenApi.OData.Authorizations;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Operation;
using Microsoft.OpenApi.OData.PathItem;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Context information for the <see cref="IEdmModel"/>, configuration, etc.
    /// </summary>
    internal class ODataContext
    {
        private IEnumerable<ODataPath> _allPaths;
        private ODataPathProvider _pathProvider;
        private HttpRequestProvider _httpRequestProvider;
        private AuthorizationProvider _authorizationProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="ODataContext"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataContext(IEdmModel model)
            : this(model, new OpenApiConvertSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataContext"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The convert setting.</param>
        public ODataContext(IEdmModel model, OpenApiConvertSettings settings)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Settings = settings ?? throw Error.ArgumentNull(nameof(settings));

            EdmSpatialTypeVisitor visitor = new EdmSpatialTypeVisitor();
            visitor.Visit(model);
            IsSpatialTypeUsed = visitor.IsSpatialTypeUsed;

            OperationHanderProvider = new OperationHandlerProvider();
            PathItemHanderProvider = new PathItemHandlerProvider();

            _authorizationProvider = new AuthorizationProvider(model);
            _httpRequestProvider = new HttpRequestProvider(model);
            _pathProvider = new ODataPathProvider(Model);

            if (settings.EnableKeyAsSegment != null)
            {
                // We have the global setting, use the global setting
                KeyAsSegment = settings.EnableKeyAsSegment.Value;
            }
            else
            {
                KeyAsSegment = false;
                if (model.EntityContainer != null)
                {
                    var keyAsSegment = model.GetKeyAsSegmentSupported(model.EntityContainer);
                    if (keyAsSegment != null)
                    {
                        KeyAsSegment = keyAsSegment.IsSupported;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the path item handler provider.
        /// </summary>
        public IPathItemHandlerProvider PathItemHanderProvider { get; }

        /// <summary>
        /// Gets the operation handler provider.
        /// </summary>
        public IOperationHandlerProvider OperationHanderProvider { get; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the Entity Container.
        /// </summary>
        public IEdmEntityContainer EntityContainer
        {
            get
            {
                return Model.EntityContainer;
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>s.
        /// </summary>
        public IEnumerable<ODataPath> AllPaths
        {
            get
            {
                if (_allPaths == null)
                {
                    _allPaths = LoadAllODataPaths();
                }

                return _allPaths;
            }
        }

        /// <summary>
        /// Gets the boolean value indicating to support key as segment.
        /// </summary>
        public bool KeyAsSegment { get; }

        /// <summary>
        /// Gets the value indicating the Edm spatial type used.
        /// </summary>
        public bool IsSpatialTypeUsed { get; private set; }

        /// <summary>
        /// Gets the convert settings.
        /// </summary>
        public OpenApiConvertSettings Settings { get; }

        /// <summary>
        /// Gets all tags.
        /// </summary>
        public IList<OpenApiTag> Tags { get; private set; }

        /// <summary>
        /// Find the Org.OData.Core.V1.HttpRequest for a given target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="method">The method name.</param>
        /// <returns>The <see cref="HttpRequest"/> or null.</returns>
        public HttpRequest FindRequest(IEdmVocabularyAnnotatable target, string method)
        {
            return _httpRequestProvider?.GetHttpRequest(target, method);
        }

        /// <summary>
        /// Gets the <see cref="Authorization"/> collections for a given target in the given Edm model.
        /// </summary>
        /// <param name="target">The Edm target.</param>
        /// <returns>The <see cref="Authorization"/> collections.</returns>
        public IEnumerable<Authorization> GetAuthorizations(IEdmVocabularyAnnotatable target)
        {
            return _authorizationProvider?.GetAuthorizations(target);
        }

        /// <summary>
        /// Append tag.
        /// </summary>
        /// <param name="tagItem">The tag item.</param>
        public void AppendTag(OpenApiTag tagItem)
        {
            if (Tags == null)
            {
                Tags = new List<OpenApiTag>();
            }

            if (Tags.Any(c => c.Name == tagItem.Name))
            {
                return;
            }

            Tags.Add(tagItem);
        }

        /// <summary>
        /// Gets all OData paths
        /// </summary>
        /// <returns>All acceptable OData path.</returns>
        private IEnumerable<ODataPath> LoadAllODataPaths()
        {
            IEnumerable<ODataPath> allPaths = _pathProvider.CreatePaths();
            foreach (var path in allPaths)
            {
                if ((path.Kind == ODataPathKind.Operation && !Settings.EnableOperationPath) ||
                    (path.Kind == ODataPathKind.OperationImport && !Settings.EnableOperationImportPath) ||
                    (path.Kind == ODataPathKind.NavigationProperty && !Settings.EnableNavigationPropertyPath))
                {
                    continue;
                }

                yield return path;
            }
        }
    }
}
