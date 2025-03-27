// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    internal class EdmOperationProvider
    {
        private readonly Lazy<IDictionary<string, IList<IEdmOperation>>> _boundEdmOperations;

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="EdmOperationProvider"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public EdmOperationProvider(IEdmModel model)
        {
            Utils.CheckArgumentNull(model, nameof(model));

            Model = model;

            _boundEdmOperations = new Lazy<IDictionary<string, IList<IEdmOperation>>>(
                LoadEdmOperations, isThreadSafe: false);
        }

        public IDictionary<string, IList<IEdmOperation>> Operations => _boundEdmOperations.Value;

        /// <summary>
        /// Find the Edm operation bounding to the given entity type.
        /// </summary>
        /// <param name="entityType">The binding entity type.</param>
        /// <param name="collection">The collection or not.</param>
        /// <returns>The found Edm operations.</returns>
        public IEnumerable<IEdmOperation>? FindOperations(IEdmEntityType entityType, bool collection)
        {
            Utils.CheckArgumentNull(entityType, nameof(entityType));

            string fullTypeName = collection ? "Collection(" + entityType.FullName() + ")" : entityType.FullName();

            if (!_boundEdmOperations.Value.TryGetValue(fullTypeName, out var edmOperations)) return null;

            foreach (IEdmEntityType derived in Model.FindAllDerivedTypes(entityType).OfType<IEdmEntityType>())
            {
                string subFullTypeName = collection ? "Collection(" + derived.FullName() + ")" : derived.FullName();

                if (_boundEdmOperations.Value.TryGetValue(subFullTypeName, out var edmSubOperations))
                {
                    foreach(var edmOperation in edmSubOperations)
                    {
                        edmOperations.Add(edmOperation);
                    }
                }
            }

            return edmOperations;
        }

        private IDictionary<string, IList<IEdmOperation>> LoadEdmOperations()
        {
            IDictionary<string, IList<IEdmOperation>> edmOperationDict = new Dictionary<string, IList<IEdmOperation>>();

            foreach (var edmOperation in Model.GetAllElements().OfType<IEdmOperation>().Where(e => e.IsBound))
            {
                IEdmOperationParameter bindingParameter = edmOperation.Parameters.First();

                string bindingTypeName = bindingParameter.Type.FullName();

                if (!edmOperationDict.TryGetValue(bindingTypeName, out var value))
                {
                    value = [];
                    edmOperationDict[bindingTypeName] = value;
                }
                value.Add(edmOperation);
            }

            return edmOperationDict;
        }
    }
}
