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
    /// The wrapper for the <see cref="IEdmModel"/>,.<see cref="OpenApiSerializerSettings"/>.
    /// </summary>
    internal class ODataContext
    {
        private IDictionary<IEdmTypeReference, IEdmOperation> _boundOperations;

        public IEdmModel Model { get; }

        public IEdmEntityContainer EntityContainer
        {
            get
            {
                return Model.EntityContainer;
            }
        }

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

        public ODataContext(IEdmModel model)
            : this(model, new OpenApiConvertSettings())
        {

        }

        public ODataContext(IEdmModel model, OpenApiConvertSettings settings)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Settings = settings ?? throw Error.ArgumentNull(nameof(settings));
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
