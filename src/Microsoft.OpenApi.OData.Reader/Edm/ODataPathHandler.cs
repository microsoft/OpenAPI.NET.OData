// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Helper class for <see cref="ODataPath"/> generating.
    /// </summary>
    internal class ODataPathHandler
    {
        private IList<ODataPath> _paths = null;

        /// <summary>
        /// Gets the OData Context
        /// </summary>
        public ODataContext Context { get; }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>s.
        /// </summary>
        public IList<ODataPath> Paths => GeneratePaths();

        /// <summary>
        /// Initializes a new instance of <see cref="ODataPathHandler"/> class.
        /// </summary>
        /// <param name="context">The OData context.</param>
        public ODataPathHandler(ODataContext context)
        {
            Context = context ?? throw Error.ArgumentNull(nameof(context));
        }

        /// <summary>
        /// Generate the <see cref="ODataPath"/> from <see cref="IEdmModel"/> and <see cref="OpenApiConvertSettings"/>.
        /// </summary>
        /// <returns>The generated paths.</returns>
        private IList<ODataPath> GeneratePaths()
        {
            if (_paths != null)
            {
                return _paths;
            }

            _paths = new List<ODataPath>();
            if (Context.Model.EntityContainer == null)
            {
                return _paths;
            }

            // entity set
            foreach (IEdmEntitySet entitySet in Context.Model.EntityContainer.EntitySets())
            {
                RetrieveNavigationSourcePaths(entitySet);

                if (Context.Settings.EnableOperationPath)
                {
                    RetrieveOperationPaths(entitySet);
                }
            }

            // singleton
            foreach (IEdmSingleton singleton in Context.Model.EntityContainer.Singletons())
            {
                RetrieveNavigationSourcePaths(singleton);

                if (Context.Settings.EnableOperationPath)
                {
                    RetrieveOperationPaths(singleton);
                }
            }

            // operation import
            if (Context.Settings.EnableOperationImportPath)
            {
                foreach (IEdmOperationImport import in Context.Model.EntityContainer.OperationImports())
                {
                    _paths.Add(new ODataPath(new ODataOperationImportSegment(import)));
                }
            }

            return _paths;
        }

        /// <summary>
        /// Retrieve the path for <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        private void RetrieveNavigationSourcePaths(IEdmNavigationSource navigationSource)
        {
            Debug.Assert(navigationSource != null);

            // navigation source itself
            ODataPath path = new ODataPath();
            path.Push(new ODataNavigationSourceSegment(navigationSource));
            _paths.Add(path.Clone()); 

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

            // for entity set, create a path with key
            if (entitySet != null)
            {
                path.Push(new ODataKeySegment(entityType));
                _paths.Add(path.Clone());
            }

            // navigation property
            if (Context.Settings.EnableNavigationPropertyPath)
            {
                foreach (IEdmNavigationProperty np in entityType.DeclaredNavigationProperties())
                {
                    RetrieveNavigationPropertyPaths(np, path);
                }
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

            int count = currentPath.GetCount(Context.Settings.CountKeySegmentAsDepth);
            if (count > Context.Settings.NavigationPropertyDepth)
            {
                return;
            }

            bool shouldExpand = ShouldExpandNavigationProperty(navigationProperty, currentPath);

            // append a navigation property.
            currentPath.Push(new ODataNavigationPropertySegment(navigationProperty));
            _paths.Add(currentPath.Clone());

            // append a navigation property key.
            IEdmEntityType navEntityType = navigationProperty.ToEntityType();
            if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
            {
                currentPath.Push(new ODataKeySegment(navEntityType));
                _paths.Add(currentPath.Clone());
            }

            if (shouldExpand)
            {
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
        /// Retrieve the <see cref="IEdmOperation"/> path for <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        private void RetrieveOperationPaths(IEdmNavigationSource navigationSource)
        {
            Debug.Assert(navigationSource != null);

            IEnumerable<Tuple<IEdmEntityType, IEdmOperation>> operations;
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;

            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(navigationSource));

            if (entitySet != null)
            {
                operations = Context.FindOperations(navigationSource.EntityType(), collection: true);
                foreach (var operation in operations)
                {
                    // Append the type cast
                    if (!operation.Item1.IsEquivalentTo(navigationSource.EntityType()))
                    {
                        path.Push(new ODataTypeCastSegment(operation.Item1));
                        path.Push(new ODataOperationSegment(operation.Item2, Context.Settings.UnqualifiedCall));
                        _paths.Add(path.Clone());
                        path.Pop();
                        path.Pop();
                    }
                    else
                    {
                        path.Push(new ODataOperationSegment(operation.Item2, Context.Settings.UnqualifiedCall));
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

            operations = Context.FindOperations(navigationSource.EntityType(), collection: false);
            foreach (var operation in operations)
            {
                // Append the type cast
                if (!operation.Item1.IsEquivalentTo(navigationSource.EntityType()))
                {
                    path.Push(new ODataTypeCastSegment(operation.Item1));
                    path.Push(new ODataOperationSegment(operation.Item2, Context.Settings.UnqualifiedCall));
                    _paths.Add(path.Clone());
                    path.Pop();
                    path.Pop();
                }
                else
                {
                    path.Push(new ODataOperationSegment(operation.Item2, Context.Settings.UnqualifiedCall));
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
