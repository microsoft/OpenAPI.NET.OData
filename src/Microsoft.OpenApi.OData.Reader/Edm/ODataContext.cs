// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
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
        private IDictionary<IEdmTypeReference, IEdmOperation> _boundOperations;
        private bool _keyAsSegmentSupported = false;
        private IList<OpenApiTag> _tags = new List<OpenApiTag>();
        private ODataPathProvider _pathProvider;
        public HttpRequestProvider _httpRequestProvider;
        public AuthorizationProvider _authorizationProvider;

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

            EdmModelVisitor visitor = new EdmModelVisitor();
            visitor.Visit(model);
            IsSpatialTypeUsed = visitor.IsSpatialTypeUsed;

            _pathProvider = new ODataPathProvider(model);

            OperationHanderProvider = new OperationHandlerProvider();
            PathItemHanderProvider = new PathItemHandlerProvider();

            _authorizationProvider = new AuthorizationProvider(model);
            _httpRequestProvider = new HttpRequestProvider(model);

            if (settings.EnableKeyAsSegment != null)
            {
                // We have the global setting, use the global setting
                _keyAsSegmentSupported = settings.EnableKeyAsSegment.Value;
            }
            else
            {
                _keyAsSegmentSupported = false;
                if (model.EntityContainer != null)
                {
                    var keyAsSegment = model.GetKeyAsSegmentSupported(model.EntityContainer);
                    if (keyAsSegment != null)
                    {
                        _keyAsSegmentSupported = keyAsSegment.IsSupported;
                    }
                }
            }
        }

        public IPathItemHandlerProvider PathItemHanderProvider { get; }

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
        public IEnumerable<ODataPath> Paths => _pathProvider.CreatePaths();

        /// <summary>
        /// Gets the boolean value indicating to support key as segment.
        /// </summary>
        public bool KeyAsSegment => _keyAsSegmentSupported;

        /// <summary>
        /// Gets the value indicating the Edm spatial type used.
        /// </summary>
        public bool IsSpatialTypeUsed { get; private set; }

        /// <summary>
        /// Gets the convert settings.
        /// </summary>
        public OpenApiConvertSettings Settings { get; }

        /// <summary>
        /// Gets the bound operations (functions & actions).
        /// </summary>
        public IDictionary<IEdmTypeReference, IEdmOperation> BoundOperations
        {
            get
            {
                if (_boundOperations == null)
                {
                    GenerateBoundOperations();
                }

                return _boundOperations;
            }
        }

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
        /// Finds the operations using the <see cref="IEdmEntityType"/>
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="collection">The collection flag.</param>
        /// <returns>The found operations.</returns>
        public IEnumerable<Tuple<IEdmEntityType, IEdmOperation>> FindOperations(IEdmEntityType entityType, bool collection)
        {
            Utils.CheckArgumentNull(entityType, nameof(entityType));

            string fullTypeName = collection ?
                "Collection(" + entityType.FullName() + ")" :
                entityType.FullName();

            foreach (var item in BoundOperations)
            {
                IEdmEntityType operationBindingType;
                if (collection)
                {
                    if (!item.Key.IsCollection())
                    {
                        continue;
                    }

                    operationBindingType = item.Key.AsCollection().ElementType().AsEntity().EntityDefinition();
                }
                else
                {
                    if (item.Key.IsCollection())
                    {
                        continue;
                    }

                    operationBindingType = item.Key.AsEntity().EntityDefinition();
                }

                if (entityType.IsAssignableFrom(operationBindingType))
                {
                    yield return Tuple.Create(operationBindingType, item.Value);
                }
            }
        }

        private void GenerateBoundOperations()
        {
            if (_boundOperations != null)
            {
                return;
            }

            _boundOperations = new Dictionary<IEdmTypeReference, IEdmOperation>();
            foreach (var edmOperation in Model.SchemaElements.OfType<IEdmOperation>().Where(e => e.IsBound))
            {
                IEdmOperationParameter bindingParameter = edmOperation.Parameters.First();
                _boundOperations.Add(bindingParameter.Type, edmOperation);
            }
        }

        public IList<OpenApiTag> Tags
        {
            get
            {
                return _tags;
            }
        }

        public void AppendTag(OpenApiTag tagItem)
        {
            if (_tags.Any(c => c.Name == tagItem.Name))
            {
                return;
            }
            _tags.Add(tagItem);
        }
    }
}
