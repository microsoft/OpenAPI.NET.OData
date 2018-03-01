// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Capabilities;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Context information for the <see cref="IEdmModel"/>, configuration, etc.
    /// </summary>
    internal class ODataContext
    {
        private IDictionary<IEdmTypeReference, IEdmOperation> _boundOperations;
        private bool _keyAsSegmentSupported = false;

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
        }

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
        /// <param name="entityType">The entity type.</param>
        /// <param name="collection">The collection flag.</param>
        /// <returns>The found operations.</returns>
        public IEnumerable<Tuple<IEdmEntityType, IEdmOperation>> FindOperations(IEdmEntityType entityType, bool collection)
        {
            string fullTypeName = collection ? "Collection(" + entityType.FullName() + ")" :
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
                /*
                if (item.Key.FullName() == fullTypeName)
                {
                    yield return item.Value;
                }*/
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
    }
}
