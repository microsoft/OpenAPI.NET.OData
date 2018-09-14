// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

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
        /// <summary>
        /// Generate the list of <see cref="ODataPath"/> with the given <see cref="IEdmModel"/>.
        /// </summary>
        /// <param name="model">The given Edm model.</param>
        /// <returns>The collection of build <see cref="ODataPath"/>.</returns>
        public static IEnumerable<ODataPath> CreatePaths(IEdmModel model)
        {
            if (model == null || model.EntityContainer == null)
            {
                return Enumerable.Empty<ODataPath>();
            }

            IList<ODataPath> paths = new List<ODataPath>();

            // entity set
            foreach (IEdmEntitySet entitySet in model.EntityContainer.EntitySets())
            {
                RetrieveNavigationSourcePaths(model, entitySet, paths);
            }

            // singleton
            foreach (IEdmSingleton singleton in model.EntityContainer.Singletons())
            {
                RetrieveNavigationSourcePaths(model, singleton, paths);
            }

            // bound operations
            RetrieveBoundOperationPaths(model, paths);

            // unbound operations
            foreach (IEdmOperationImport import in model.EntityContainer.OperationImports())
            {
                paths.Add(new ODataPath(new ODataOperationImportSegment(import)));
            }

            return paths;
        }

        /// <summary>
        /// Retrieve the paths for <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="paths">The existing paths.</param>
        private static void RetrieveNavigationSourcePaths(IEdmModel model, IEdmNavigationSource navigationSource, IList<ODataPath> paths)
        {
            Debug.Assert(paths != null);
            Debug.Assert(model != null);
            Debug.Assert(navigationSource != null);

            // navigation source itself
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(navigationSource));
            paths.Add(path);

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

            // for entity set, create a path with key
            if (entitySet != null)
            {
                path.Push(new ODataKeySegment(entityType));
                paths.Add(path.Clone());
            }

            // navigation property
            foreach (IEdmNavigationProperty np in entityType.DeclaredNavigationProperties())
            {
                RetrieveNavigationPropertyPaths(model, np, path, paths);
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
        private static void RetrieveNavigationPropertyPaths(IEdmModel model, IEdmNavigationProperty navigationProperty, ODataPath currentPath, IList<ODataPath> paths)
        {
            Debug.Assert(navigationProperty != null);
            Debug.Assert(currentPath != null);

            bool shouldExpand = ShouldExpandNavigationProperty(model, navigationProperty, currentPath);

            // append a navigation property.
            currentPath.Push(new ODataNavigationPropertySegment(navigationProperty));
            paths.Add(currentPath.Clone());

            // append a navigation property key.
            IEdmEntityType navEntityType = navigationProperty.ToEntityType();
            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Push(new ODataKeySegment(navEntityType));
                paths.Add(currentPath.Clone());
            }

            if (shouldExpand)
            {
                foreach (IEdmNavigationProperty subNavProperty in navEntityType.DeclaredNavigationProperties())
                {
                    RetrieveNavigationPropertyPaths(model, subNavProperty, currentPath, paths);
                }
            }

            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Pop();
            }

            currentPath.Pop();
        }

        private static bool ShouldExpandNavigationProperty(IEdmModel model, IEdmNavigationProperty navigationProperty, ODataPath currentPath)
        {
            // only expand for the containment.
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

            if (FindNavigationSource(model, navEntityType).Any())
            {
                return false;
            }


            return true;
        }

        private static void RetrieveBoundOperationPaths(IEdmModel model, IList<ODataPath> paths)
        {
            IList<ODataPath> npPaths = paths.Where(p => p.Kind == ODataPathKind.NavigationProperty).ToList();

            var navigationSourceDic = model.EntityContainer.EntitySets().ToDictionary(o => o.EntityType(), o => o as IEdmNavigationSource);

            foreach (var edmOperation in model.SchemaElements.OfType<IEdmOperation>().Where(e => e.IsBound))
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
                var correspondingNavigationSource = FindNavigationSource(model, bindingEntityType);
                if (correspondingNavigationSource.Any())
                {
                    foreach(var ns in correspondingNavigationSource)
                    {
                        if (isCollection)
                        {
                            if (ns is IEdmEntitySet)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataOperationSegment(edmOperation));
                                paths.Add(newPath);
                                found = true;
                            }
                        }
                        else
                        {
                            if (ns is IEdmSingleton)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataOperationSegment(edmOperation));
                                paths.Add(newPath);
                                found = true;
                            }
                            else
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataKeySegment(ns.EntityType()),
                                    new ODataOperationSegment(edmOperation));
                                paths.Add(newPath);
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
                // IList<ODataPath> npPaths = paths.Where(p => p.Kind == ODataPathKind.NavigationProperty).ToList();
                foreach(var path in npPaths)
                {
                    ODataNavigationPropertySegment npSegment = path.Segments.Last(s => s is ODataNavigationPropertySegment) as ODataNavigationPropertySegment;
                    IEdmEntityType navPropertyEntityType = npSegment.NavigationProperty.ToEntityType();
                    if (navPropertyEntityType != bindingEntityType)
                    {
                        continue;
                    }

                    bool isLastKeySegment = path.LastSegment is ODataKeySegment;
                    if ((isCollection && !isLastKeySegment) || (!isCollection && isLastKeySegment))
                    {
                        ODataPath newPath = path.Clone();
                        newPath.Push(new ODataOperationSegment(edmOperation));
                        paths.Add(newPath);
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
                    var baseNavigationSource = FindNavigationSource(model, baseType);
                    if (baseNavigationSource.Any())
                    {
                        foreach (var ns in baseNavigationSource)
                        {
                            if (isCollection)
                            {
                                if (ns is IEdmEntitySet)
                                {
                                    ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType),
                                        new ODataOperationSegment(edmOperation));
                                    paths.Add(newPath);
                                    found = true;
                                }
                            }
                            else
                            {
                                if (ns is IEdmSingleton)
                                {
                                    ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType), new ODataOperationSegment(edmOperation));
                                    paths.Add(newPath);
                                    found = true;
                                }
                                else
                                {
                                    ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataKeySegment(ns.EntityType()), new ODataTypeCastSegment(bindingEntityType),
                                        new ODataOperationSegment(edmOperation));
                                    paths.Add(newPath);
                                    found = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<IEdmNavigationSource> FindNavigationSource(IEdmModel model, IEdmEntityType entityType)
        {
            IEnumerable<IEdmNavigationSource> returnEnumerable1 = model.EntityContainer.EntitySets().Where(e => e.EntityType() == entityType);
            IEnumerable<IEdmNavigationSource> returnEnumerable2 = model.EntityContainer.Singletons().Where(e => e.EntityType() == entityType);
            return returnEnumerable1.Concat(returnEnumerable2);
        }
    }
}
