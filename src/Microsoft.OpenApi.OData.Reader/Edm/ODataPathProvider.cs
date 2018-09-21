// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Provide class for <see cref="ODataPath"/> generating.
    /// </summary>
    public class ODataPathProvider
    {
        private List<ODataPath> _allODataPaths;
        private Lazy<IDictionary<IEdmEntityType, IList<IEdmNavigationSource>>> _allNavigationSource;

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ODataPathProvider"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataPathProvider(IEdmModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));

            // if the entity container is null or empty. assign the path as empty.
            if (model.EntityContainer == null)
            {
                _allODataPaths = new List<ODataPath>();
            }

            _allNavigationSource = new Lazy<IDictionary<IEdmEntityType, IList<IEdmNavigationSource>>>(
                () => model.LoadAllNavigationSources(), false);
        }

        /// <summary>
        /// Generate the list of <see cref="ODataPath"/>.
        /// </summary>
        /// <returns>The collection of build <see cref="ODataPath"/>.</returns>
        public virtual IEnumerable<ODataPath> CreatePaths()
        {
            if (_allODataPaths != null)
            {
                return _allODataPaths;
            }

            _allODataPaths = new List<ODataPath>();

            // entity set
            foreach (IEdmEntitySet entitySet in Model.EntityContainer.EntitySets())
            {
                RetrieveNavigationSourcePaths(entitySet);
            }

            // singleton
            foreach (IEdmSingleton singleton in Model.EntityContainer.Singletons())
            {
                RetrieveNavigationSourcePaths(singleton);
            }

            // bound operations
            RetrieveBoundOperationPaths();

            // unbound operations
            foreach (IEdmOperationImport import in Model.EntityContainer.OperationImports())
            {
                _allODataPaths.Add(new ODataPath(new ODataOperationImportSegment(import)));
            }

            _allODataPaths.Sort();

            return _allODataPaths;
        }

        /// <summary>
        /// Retrieve the paths for <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        private void RetrieveNavigationSourcePaths(IEdmNavigationSource navigationSource)
        {
            Debug.Assert(navigationSource != null);

            // navigation source itself
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(navigationSource));
            _allODataPaths.Add(path.Clone());

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

            // for entity set, create a path with key
            if (entitySet != null)
            {
                path.Push(new ODataKeySegment(entityType));
                _allODataPaths.Add(path.Clone());
            }

            // navigation property
            foreach (IEdmNavigationProperty np in entityType.DeclaredNavigationProperties())
            {
                RetrieveNavigationPropertyPaths(np, path);
            }

            if (entitySet != null)
            {
                path.Pop(); // end of entity
            }

            path.Pop(); // end of navigation source.
            Debug.Assert(path.Any() == false);
        }

        /// <summary>
        /// Retrieve the path for <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="currentPath">The current OData path.</param>
        private void RetrieveNavigationPropertyPaths(IEdmNavigationProperty navigationProperty, ODataPath currentPath)
        {
            Debug.Assert(navigationProperty != null);
            Debug.Assert(currentPath != null);

            // test ahead for the navigation expandable.
            bool shouldExpand = ShouldExpandNavigationProperty(navigationProperty, currentPath);

            // append a navigation property.
            currentPath.Push(new ODataNavigationPropertySegment(navigationProperty));
            _allODataPaths.Add(currentPath.Clone());

            // append a navigation property key.
            IEdmEntityType navEntityType = navigationProperty.ToEntityType();
            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Push(new ODataKeySegment(navEntityType));
                _allODataPaths.Add(currentPath.Clone());
            }

            if (shouldExpand)
            {
                // expand to sub navigation properties
                foreach (IEdmNavigationProperty subNavProperty in navEntityType.DeclaredNavigationProperties())
                {
                    RetrieveNavigationPropertyPaths(subNavProperty, currentPath);
                }
            }

            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Pop();
            }

            currentPath.Pop();
        }

        /// <summary>
        /// Test the navigation property should be expanded or not.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="currentPath">The current OData path.</param>
        private bool ShouldExpandNavigationProperty(IEdmNavigationProperty navigationProperty, ODataPath currentPath)
        {
            Debug.Assert(navigationProperty != null);
            Debug.Assert(currentPath != null);

            // not expand for the non-containment.
            if (!navigationProperty.ContainsTarget)
            {
                return false;
            }

            // check the type is visited before, if visited, not expand it.
            IEdmEntityType navEntityType = navigationProperty.ToEntityType();
            foreach (ODataSegment segment in currentPath)
            {
                if (navEntityType.IsAssignableFrom(segment.EntityType))
                {
                    return false;
                }
            }

            // check whether the navigation type used to define a navigation source.
            // if so, not expand it.
            return !_allNavigationSource.Value.ContainsKey(navEntityType);
        }

        private void RetrieveBoundOperationPaths()
        {
            IList<ODataPath> npPaths = _allODataPaths.Where(p => p.Kind == ODataPathKind.NavigationProperty).ToList();
            IDictionary<IEdmEntityType, IList<ODataPath>> allNavigationPropertyPaths = new Dictionary<IEdmEntityType, IList<ODataPath>>();
            foreach(var path in npPaths)
            {
                ODataNavigationPropertySegment npSegment = path.Segments.Last(s => s is ODataNavigationPropertySegment) as ODataNavigationPropertySegment;
                IEdmEntityType navPropertyEntityType = npSegment.NavigationProperty.ToEntityType();

                if (!allNavigationPropertyPaths.TryGetValue(navPropertyEntityType, out IList<ODataPath> value))
                {
                    value = new List<ODataPath>();
                    allNavigationPropertyPaths[navPropertyEntityType] = value;
                }

                value.Add(path);
            }

            foreach (var edmOperation in Model.SchemaElements.OfType<IEdmOperation>().Where(e => e.IsBound))
            {
                IEdmOperationParameter bindingParameter = edmOperation.Parameters.First();
                IEdmTypeReference bindingType = bindingParameter.Type;

                bool isCollection = bindingType.IsCollection();

                if (isCollection)
                {
                    bindingType = bindingType.AsCollection().ElementType();
                }
                if (!bindingType.IsEntity())
                {
                    continue;
                }

                IEdmEntityType bindingEntityType = bindingType.AsEntity().EntityDefinition();

                bool found = false;

                // 1. Search for correspoinding navigation source
                if (_allNavigationSource.Value.TryGetValue(bindingEntityType, out IList<IEdmNavigationSource> correspondingNavigationSources))
                {
                    foreach(var ns in correspondingNavigationSources)
                    {
                        if (isCollection)
                        {
                            if (ns is IEdmEntitySet)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataOperationSegment(edmOperation));
                                _allODataPaths.Add(newPath);
                                found = true;
                            }
                        }
                        else
                        {
                            if (ns is IEdmSingleton)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataOperationSegment(edmOperation));
                                _allODataPaths.Add(newPath);
                                found = true;
                            }
                            else
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataKeySegment(ns.EntityType()),
                                    new ODataOperationSegment(edmOperation));
                                _allODataPaths.Add(newPath);
                                found = true;
                            }
                        }
                    }
                }

                if (found)
                {
                    continue;
                }

                // 2. Search for generated navigation property
                /*
                foreach (var path in npPaths)
                {
                    ODataNavigationPropertySegment npSegment = path.Segments.Last(s => s is ODataNavigationPropertySegment) as ODataNavigationPropertySegment;
                    IEdmEntityType navPropertyEntityType = npSegment.NavigationProperty.ToEntityType();
                    if (navPropertyEntityType != bindingEntityType)
                    {
                        continue;
                    }

                    bool isLastKeySegment = path.LastSegment is ODataKeySegment;

                    if (isCollection)
                    {
                        if (isLastKeySegment)
                        {
                            continue;
                        }

                        if (npSegment.NavigationProperty.TargetMultiplicity() != EdmMultiplicity.Many)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!isLastKeySegment && npSegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                        {
                            continue;
                        }
                    }

                    ODataPath newPath = path.Clone();
                    newPath.Push(new ODataOperationSegment(edmOperation));
                    _allODataPaths.Add(newPath);
                    found = true;
                }*/

                if (allNavigationPropertyPaths.TryGetValue(bindingEntityType, out IList<ODataPath> value))
                {
                    foreach(var path in value)
                    {
                        ODataNavigationPropertySegment npSegment = path.Segments.Last(s => s is ODataNavigationPropertySegment) as ODataNavigationPropertySegment;

                        bool isLastKeySegment = path.LastSegment is ODataKeySegment;

                        if (isCollection)
                        {
                            if (isLastKeySegment)
                            {
                                continue;
                            }

                            if (npSegment.NavigationProperty.TargetMultiplicity() != EdmMultiplicity.Many)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!isLastKeySegment && npSegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                            {
                                continue;
                            }
                        }

                        ODataPath newPath = path.Clone();
                        newPath.Push(new ODataOperationSegment(edmOperation));
                        _allODataPaths.Add(newPath);
                        found = true;
                    }
                }

                if (found)
                {
                    continue;
                }

                // 3. Search for derived
                foreach(var baseType in bindingEntityType.FindAllBaseTypes())
                {
                    if (_allNavigationSource.Value.TryGetValue(baseType, out IList<IEdmNavigationSource> baseNavigationSource))
                    {
                        foreach (var ns in baseNavigationSource)
                        {
                            if (isCollection)
                            {
                                if (ns is IEdmEntitySet)
                                {
                                    ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType),
                                        new ODataOperationSegment(edmOperation));
                                    _allODataPaths.Add(newPath);
                                    found = true;
                                }
                            }
                            else
                            {
                                if (ns is IEdmSingleton)
                                {
                                    ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType), new ODataOperationSegment(edmOperation));
                                    _allODataPaths.Add(newPath);
                                    found = true;
                                }
                                else
                                {
                                    ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataKeySegment(ns.EntityType()), new ODataTypeCastSegment(bindingEntityType),
                                        new ODataOperationSegment(edmOperation));
                                    _allODataPaths.Add(newPath);
                                    found = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
