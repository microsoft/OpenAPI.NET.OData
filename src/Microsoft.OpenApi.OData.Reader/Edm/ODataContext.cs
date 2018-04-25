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
        private IList<Authorization> _authorizations;
        private IList<OpenApiTag> _tags = new List<OpenApiTag>();
        private ODataPathHandler _pathHandler;

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

            _keyAsSegmentSupported = settings.KeyAsSegment ?? model.GetKeyAsSegmentSupported();

            _pathHandler = new ODataPathHandler(this);

            OperationHanderProvider = new OperationHandlerProvider();
            PathItemHanderProvider = new PathItemHandlerProvider();
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
        public IList<ODataPath> Paths => _pathHandler.Paths;

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

        private IDictionary<IEdmVocabularyAnnotatable, HttpRequestsAnnotation> _requests;

        public HttpRequest FindRequest(IEdmVocabularyAnnotatable target, string method)
        {
            if (_requests == null)
            {
                _requests = new Dictionary<IEdmVocabularyAnnotatable, HttpRequestsAnnotation>();
            }

            if (!_requests.TryGetValue(target, out HttpRequestsAnnotation value))
            {
                value = new HttpRequestsAnnotation(Model, target);
                _requests.Add(target, value);
            }

            return value.GetRequest(method);
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

        /// <summary>
        /// Gets the Authorizations
        /// </summary>
        public IList<Authorization> Authorizations
        {
            get
            {
                if (_authorizations == null)
                {
                    RetrieveAuthorizations();
                }

                return _authorizations;
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

        private void RetrieveAuthorizations()
        {
            if (_authorizations != null)
            {
                return;
            }
            _authorizations = new List<Authorization>();
            if (Model.EntityContainer == null)
            {
                return;
            }

            IEdmVocabularyAnnotation annotation = Model.GetVocabularyAnnotation(Model.EntityContainer, AuthorizationConstants.Authorizations);
            if (annotation == null ||
                annotation.Value == null ||
                annotation.Value.ExpressionKind != EdmExpressionKind.Collection)
            {
                return;
            }

            IEdmCollectionExpression collection = (IEdmCollectionExpression)annotation.Value;

            foreach (var item in collection.Elements)
            {
                IEdmRecordExpression record = item as IEdmRecordExpression;
                if (record == null || record.DeclaredType == null)
                {
                    continue;
                }

                Authorization auth = Authorization.CreateAuthorization(record);
                if (auth != null)
                {
                    _authorizations.Add(auth);
                }
            }
        }

        private IDictionary<string, int> _cached1 = new Dictionary<string, int>();
        public int GetIndex(string source)
        {
            if (_cached1.TryGetValue(source, out int value))
            {
                _cached1[source]++;
                return _cached1[source];
            }
            else
            {
                _cached1[source] = 0;
                return 0;
            }
        }
    }
}
