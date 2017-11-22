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
    internal class OpenApiContext
    {
        private IDictionary<IEdmTypeReference, IEdmOperation> _boundOperations;

        public IEdmModel Model { get; }

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

        private OpenApiContext(IEdmModel model, OpenApiConvertSettings settings)
        {
            Model = model ?? throw Error.ArgumentNull(nameof(model));
            Settings = settings ?? throw Error.ArgumentNull(nameof(settings));
        }

        public static OpenApiContext CreateContext(IEdmModel model)
        {
            return CreateContext(model, new OpenApiConvertSettings());
        }

        public static OpenApiContext CreateContext(IEdmModel model, OpenApiConvertSettings settings)
        {
            return new OpenApiContext(model, settings);
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
