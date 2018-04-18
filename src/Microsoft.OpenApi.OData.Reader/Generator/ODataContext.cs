// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Authorizations;
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
        private IList<Authorization> _authorizations;
        private IList<ODataPath> _paths;
        private IList<OpenApiTag> _tags = new List<OpenApiTag>();

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

        public IList<ODataPath> Paths
        {
            get
            {
                if (_paths == null)
                {
                    RetrievePaths();
                }

                return _paths ?? throw Error.ArgumentNull("Paths");
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

        private void RetrievePaths()
        {
            if (_paths != null)
            {
                return;
            }

            _paths = new List<ODataPath>();

            if (Model.EntityContainer == null)
            {
                return;
            }

            foreach (IEdmEntitySet entitySet in Model.EntityContainer.EntitySets())
            {
                RetrievePaths(entitySet);
                RetrieveOperationPaths(entitySet);
            }

            foreach(IEdmSingleton singleton in Model.EntityContainer.Singletons())
            {
                RetrievePaths(singleton);
                RetrieveOperationPaths(singleton);
            }

            foreach(IEdmOperationImport import in Model.EntityContainer.OperationImports())
            {
                _paths.Add(new ODataPath(new ODataOperationImportSegment(import)));
            }
        }

        private void RetrieveOperationPaths(IEdmNavigationSource navigationSource)
        {
            IEnumerable<Tuple<IEdmEntityType, IEdmOperation>> operations;
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(navigationSource));

            if (entitySet != null)
            {
                operations = FindOperations(navigationSource.EntityType(), collection: true);
                foreach (var operation in operations)
                {
                    // Append the type cast
                    if (!operation.Item1.IsEquivalentTo(navigationSource.EntityType()))
                    {
                        path.Push(new ODataTypeCastSegment(operation.Item1));
                        path.Push(new ODataOperationSegment(operation.Item2, Settings.UnqualifiedCall));
                        _paths.Add(path.Clone());
                        path.Pop();
                        path.Pop();
                    }
                    else
                    {
                        path.Push(new ODataOperationSegment(operation.Item2, Settings.UnqualifiedCall));
                        _paths.Add(path.Clone());
                        path.Pop();
                    }
                }
            }

            // for single
            if (entitySet != null)
            {
                path.Push(new ODataKeySegment(navigationSource.EntityType()));
            }

            operations = FindOperations(navigationSource.EntityType(), collection: false);
            foreach (var operation in operations)
            {
                // Append the type cast
                if (!operation.Item1.IsEquivalentTo(navigationSource.EntityType()))
                {
                    path.Push(new ODataTypeCastSegment(operation.Item1));
                    path.Push(new ODataOperationSegment(operation.Item2, Settings.UnqualifiedCall));
                    _paths.Add(path.Clone());
                    path.Pop();
                    path.Pop();
                }
                else
                {
                    path.Push(new ODataOperationSegment(operation.Item2, Settings.UnqualifiedCall));
                    _paths.Add(path.Clone());
                    path.Pop();
                }
            }

            if (entitySet != null)
            {
                path.Pop();
            }

            path.Pop();

            Debug.Assert(path.Any() == false);
        }

        private void RetrievePaths(IEdmNavigationSource navigationSource)
        {
            ODataPath path = new ODataPath();
            path.Push(new ODataNavigationSourceSegment(navigationSource));
            _paths.Add(path.Clone()); // navigation source itself

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

            if (entitySet != null)
            {
                path.Push(new ODataKeySegment(entityType));
                _paths.Add(path.Clone());
            }

            foreach (IEdmNavigationProperty np in entityType.DeclaredNavigationProperties())
            {
                RetrievePaths(np, path);
            }

            if (entitySet != null)
            {
                path.Pop();
            }

            path.Pop();

            Debug.Assert(path.Any() == false);
        }

        private void RetrievePaths(IEdmNavigationProperty navigationProperty, ODataPath currentPath)
        {
            if (currentPath.Count > Settings.NavigationPropertyDepth)
            {
                return;
            }

            bool shouldExpand = ShouldExpandNavigationProperty(navigationProperty, currentPath);

            currentPath.Push(new ODataNavigationPropertySegment(navigationProperty));
            _paths.Add(currentPath.Clone());

            IEdmEntityType navEntityType = navigationProperty.ToEntityType();

            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Push(new ODataKeySegment(navEntityType));
                _paths.Add(currentPath.Clone());
            }

            //////////////////////////////////////////
            if (shouldExpand)
            {
                foreach (IEdmNavigationProperty subNavProperty in navEntityType.DeclaredNavigationProperties())
                {
                    RetrievePaths(subNavProperty, currentPath);
                }
            }
            //////////////////////////////////////////

            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Pop();
            }

            currentPath.Pop();
        }

        private static bool ShouldExpandNavigationProperty(IEdmNavigationProperty navigationProperty, ODataPath currentPath)
        {
            if (!navigationProperty.ContainsTarget)
            {
                return false;
            }

            IEdmEntityType navEntityType = navigationProperty.ToEntityType();
            foreach (ODataSegment segment in currentPath)
            {
                if (navEntityType.IsAssignableFrom(segment.EntityType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
