// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Context information for the <see cref="IEdmModel"/>, configuration, etc.
    /// </summary>
    internal class ODataContext
    {
        private IDictionary<IEdmTypeReference, IEdmOperation> _boundOperations;

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
        /// Gets the value indicating the Edm spatial type used.
        /// </summary>
        public bool IsSpatialTypeUsed { get; private set; }

        /// <summary>
        /// Gets the convert settings.
        /// </summary>
        public OpenApiConvertSettings Settings { get; }

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

        public IEnumerable<IEdmOperation> FindOperations(IEdmEntityType entityType, bool collection)
        {
            string fullTypeName = collection ? "Collection(" + entityType.FullName() + ")" :
                entityType.FullName();

            foreach (var item in BoundOperations)
            {
                if (item.Key.FullName() == fullTypeName)
                {
                    yield return item.Value;
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
    }
}
