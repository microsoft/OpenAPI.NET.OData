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
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Provide class for <see cref="ODataPath"/> generating.
    /// </summary>
    public class ODataPathProvider : IODataPathProvider
    {
        private IDictionary<IEdmEntityType, IList<IEdmNavigationSource>> _allNavigationSources;

        private IDictionary<IEdmEntityType, IList<ODataPath>> _allNavigationSourcePaths =
            new Dictionary<IEdmEntityType, IList<ODataPath>>();

        private IDictionary<IEdmEntityType, IList<ODataPath>> _allNavigationPropertyPaths =
            new Dictionary<IEdmEntityType, IList<ODataPath>>();

        private IList<ODataPath> _allOperationPaths = new List<ODataPath>();

        private IEdmModel _model;

        /// <summary>
        /// Can filter the <see cref="IEdmElement"/> or not.
        /// </summary>
        /// <param name="element">The Edm element.</param>
        /// <returns>True/false.</returns>
        public virtual bool CanFilter(IEdmElement element) => true;

        /// <summary>
        /// Generate the list of <see cref="ODataPath"/> based on the given <see cref="IEdmModel"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The conversion settings.</param>
        /// <returns>The collection of built <see cref="ODataPath"/>.</returns>
        public virtual IEnumerable<ODataPath> GetPaths(IEdmModel model, OpenApiConvertSettings settings)
       {
           if (model == null || model.EntityContainer == null)
           {
               return Enumerable.Empty<ODataPath>();
           }

           Initialize(model);

           // entity set
           foreach (IEdmEntitySet entitySet in _model.EntityContainer.EntitySets())
           {
               if (CanFilter(entitySet))
               {
                   RetrieveNavigationSourcePaths(entitySet);
               }
           }

           // singleton
           foreach (IEdmSingleton singleton in _model.EntityContainer.Singletons())
           {
               if (CanFilter(singleton))
               {
                   RetrieveNavigationSourcePaths(singleton);
               }
           }

           // bound operations
           RetrieveBoundOperationPaths(settings);

           // unbound operations
           foreach (IEdmOperationImport import in _model.EntityContainer.OperationImports())
           {
               if (CanFilter(import))
               {
                   AppendPath(new ODataPath(new ODataOperationImportSegment(import)));
               }
           }

           return MergePaths();
        }

        /// <summary>
        /// Initialize the provider.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        protected virtual void Initialize(IEdmModel model)
        {
            Debug.Assert(model != null);

            _model = model;
            _allNavigationSources = model.LoadAllNavigationSources();
            _allNavigationSourcePaths.Clear();
            _allNavigationPropertyPaths.Clear();
            _allOperationPaths.Clear();
        }

       private IEnumerable<ODataPath> MergePaths()
       {
           List<ODataPath> allODataPaths = new List<ODataPath>();
           foreach (var item in _allNavigationSourcePaths.Values)
           {
               allODataPaths.AddRange(item);
           }

           foreach (var item in _allNavigationPropertyPaths.Values)
           {
               allODataPaths.AddRange(item);
           }

           allODataPaths.AddRange(_allOperationPaths);

           allODataPaths.Sort();

           return allODataPaths;
       }

        private void AppendPath(ODataPath path)
        {
            Debug.Assert(path != null);

            ODataPathKind kind = path.Kind;
            switch(kind)
            {
                case ODataPathKind.Entity:
                case ODataPathKind.EntitySet:
                case ODataPathKind.Singleton:
                case ODataPathKind.MediaEntity:
                    ODataNavigationSourceSegment navigationSourceSegment = (ODataNavigationSourceSegment)path.FirstSegment;
                    if (!_allNavigationSourcePaths.TryGetValue(navigationSourceSegment.EntityType, out IList<ODataPath> nsList))
                    {
                        nsList = new List<ODataPath>();
                        _allNavigationSourcePaths[navigationSourceSegment.EntityType] = nsList;
                    }

                    nsList.Add(path);
                    break;

                case ODataPathKind.NavigationProperty:
                case ODataPathKind.Ref:
                    ODataNavigationPropertySegment navigationPropertySegment = path.Last(p => p is ODataNavigationPropertySegment)
                        as ODataNavigationPropertySegment;

                    if (!_allNavigationPropertyPaths.TryGetValue(navigationPropertySegment.EntityType, out IList<ODataPath> npList))
                    {
                        npList = new List<ODataPath>();
                        _allNavigationPropertyPaths[navigationPropertySegment.EntityType] = npList;
                    }

                    npList.Add(path);
                    break;

                case ODataPathKind.Operation:
                case ODataPathKind.OperationImport:
                    _allOperationPaths.Add(path);
                    break;

                default:
                    return;
            }
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
            AppendPath(path.Clone());

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

            // for entity set, create a path with key
            if (entitySet != null)
            {
                path.Push(new ODataKeySegment(entityType));
                AppendPath(path.Clone());
            }

            // media entity
            RetrieveMediaEntityStreamPaths(entityType, path);

            // navigation property
            foreach (IEdmNavigationProperty np in entityType.DeclaredNavigationProperties())
            {
                if (CanFilter(np))
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
        /// Retrieves the paths for a media entity stream.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="currentPath">The current OData path.</param>
        private void RetrieveMediaEntityStreamPaths(IEdmEntityType entityType, ODataPath currentPath)
        {
            Debug.Assert(entityType != null);
            Debug.Assert(currentPath != null);

            bool createValuePath = true;
            foreach (IEdmStructuralProperty sp in entityType.DeclaredStructuralProperties())
            {
                if (sp.Type.AsPrimitive().IsStream())
                {
                    currentPath.Push(new ODataStreamPropertySegment(sp.Name));
                    AppendPath(currentPath.Clone());
                    currentPath.Pop();
                }

                if (sp.Name.Equals("content", System.StringComparison.OrdinalIgnoreCase))
                {
                    createValuePath = false;
                }
            }

            /* Create a /$value path only if entity has stream and
             * does not contain a structural property named Content
             */
            if (createValuePath && entityType.HasStream)
            {
                currentPath.Push(new ODataStreamContentSegment());
                AppendPath(currentPath.Clone());
                currentPath.Pop();
            }
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

            // Check whether the navigation property should be part of the path
            NavigationRestrictionsType navigation = _model.GetRecord<NavigationRestrictionsType>(navigationProperty, CapabilitiesConstants.NavigationRestrictions);
            if (navigation != null && !navigation.IsNavigable)
            {
                return;
            }

            // test the expandable for the navigation property.
            bool shouldExpand = ShouldExpandNavigationProperty(navigationProperty, currentPath);

            // append a navigation property.
            currentPath.Push(new ODataNavigationPropertySegment(navigationProperty));
            AppendPath(currentPath.Clone());

            // Check whether a collection-valued navigation property should be indexed by key value(s).
            NavigationPropertyRestriction restriction = navigation?.RestrictedProperties?.FirstOrDefault();

            if (restriction == null || restriction.IndexableByKey == true)
            {
              //  IEdmEntityType navEntityType = navigationProperty.ToEntityType();

                //if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                //{
                //    // Append a navigation property key.
                //    currentPath.Push(new ODataKeySegment(navEntityType));
                //    AppendPath(currentPath.Clone());
                //}

                if (!navigationProperty.ContainsTarget)
                {
                    // Non-Contained
                    // Single-Valued:  DELETE ~/entityset/{key}/single-valued-Nav/$ref
                    // collection-valued:   DELETE ~/entityset/{key}/collection-valued-Nav/$ref?$id ={ navKey}
                    ODataPath newPath = currentPath.Clone();
                    newPath.Push(ODataRefSegment.Instance); // $ref
                    AppendPath(newPath);

                    if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {

                    }
                }
                else
                {
                    IEdmEntityType navEntityType = navigationProperty.ToEntityType();

                    // append a navigation property key.
                    if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        currentPath.Push(new ODataKeySegment(navEntityType));
                        AppendPath(currentPath.Clone());

                        if (!navigationProperty.ContainsTarget)
                        {
                            // TODO: Shall we add "$ref" after {key}, and only support delete?
                            // ODataPath newPath = currentPath.Clone();
                            // newPath.Push(ODataRefSegment.Instance); // $ref
                            // AppendPath(newPath);
                        }
                    }

                    // Get possible stream paths for the navigation entity type
                    RetrieveMediaEntityStreamPaths(navEntityType, currentPath);

                    if (shouldExpand)
                    {
                        // expand to sub navigation properties
                        foreach (IEdmNavigationProperty subNavProperty in navEntityType.DeclaredNavigationProperties())
                        {
                            if (CanFilter(subNavProperty))
                            {
                                RetrieveNavigationPropertyPaths(subNavProperty, currentPath);
                            }
                        }
                    }

                    if (navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        currentPath.Pop();
                    }
                }
            }
            currentPath.Pop();
        }

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

            return true;
        }

        /// <summary>
        /// Retrieve all bounding <see cref="IEdmOperation"/>.
        /// </summary>
        private void RetrieveBoundOperationPaths(OpenApiConvertSettings convertSettings)
        {
            foreach (var edmOperation in _model.GetAllElements().OfType<IEdmOperation>().Where(e => e.IsBound))
            {
                if (!CanFilter(edmOperation))
                {
                    continue;
                }

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

                var firstEntityType = bindingType.AsEntity().EntityDefinition();
                var allEntitiesForOperation= new List<IEdmEntityType>(){ firstEntityType };

                System.Func<IEdmNavigationSource, bool> filter = (z) =>
                    z.EntityType() != firstEntityType &&
                    z.EntityType().FindAllBaseTypes().Contains(firstEntityType);

                //Search all EntitySets
                allEntitiesForOperation.AddRange(
                    _model.EntityContainer.EntitySets()
                            .Where(filter).Select(x => x.EntityType())
                );

                //Search all singletons
                allEntitiesForOperation.AddRange(
                    _model.EntityContainer.Singletons()
                            .Where(filter).Select(x => x.EntityType())
                );

                allEntitiesForOperation = allEntitiesForOperation.Distinct().ToList();

                foreach (var bindingEntityType in allEntitiesForOperation)
                {
                    // 1. Search for corresponding navigation source path
                    if (AppendBoundOperationOnNavigationSourcePath(edmOperation, isCollection, bindingEntityType))
                    {
                        continue;
                    }

                    // 2. Search for generated navigation property
                    if (AppendBoundOperationOnNavigationPropertyPath(edmOperation, isCollection, bindingEntityType))
                    {
                        continue;
                    }

                    // 3. Search for derived
                    if (AppendBoundOperationOnDerived(edmOperation, isCollection, bindingEntityType, convertSettings))
                    {
                        continue;
                    }

                    // 4. Search for derived generated navigation property
                    if (AppendBoundOperationOnDerivedNavigationPropertyPath(edmOperation, isCollection, bindingEntityType, convertSettings))
                    {
                        continue;
                    }

                }
            }
        }

        private bool AppendBoundOperationOnNavigationSourcePath(IEdmOperation edmOperation, bool isCollection, IEdmEntityType bindingEntityType)
        {
            bool found = false;

            if (_allNavigationSourcePaths.TryGetValue(bindingEntityType, out IList<ODataPath> value))
            {
                bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

                foreach (var subPath in value)
                {
                    if ((isCollection && subPath.Kind == ODataPathKind.EntitySet) ||
                            (!isCollection && subPath.Kind != ODataPathKind.EntitySet &&
                                subPath.Kind != ODataPathKind.MediaEntity))
                    {
                        ODataPath newPath = subPath.Clone();
                        newPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction));
                        AppendPath(newPath);
                        found = true;
                    }
                }
            }

            return found;
        }

        private bool AppendBoundOperationOnNavigationPropertyPath(IEdmOperation edmOperation, bool isCollection, IEdmEntityType bindingEntityType)
        {
            bool found = false;
            bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

            if (_allNavigationPropertyPaths.TryGetValue(bindingEntityType, out IList<ODataPath> value))
            {
                foreach (var path in value)
                {
                    if (path.Kind == ODataPathKind.Ref)
                    {
                        continue;
                    }

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
                    newPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction));
                    AppendPath(newPath);
                    found = true;
                }
            }

            return found;
        }

        private bool AppendBoundOperationOnDerived(
            IEdmOperation edmOperation,
            bool isCollection,
            IEdmEntityType bindingEntityType,
            OpenApiConvertSettings convertSettings)
        {
            bool found = false;

            bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);
            foreach (var baseType in bindingEntityType.FindAllBaseTypes())
            {
                if (_allNavigationSources.TryGetValue(baseType, out IList<IEdmNavigationSource> baseNavigationSource))
                {
                    foreach (var ns in baseNavigationSource)
                    {
                        if (HasUnsatisfiedDerivedTypeConstraint(
                            ns as IEdmVocabularyAnnotatable,
                            baseType,
                            convertSettings))
                        {
                            continue;
                        }

                        if (isCollection)
                        {
                            if (ns is IEdmEntitySet)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType),
                                    new ODataOperationSegment(edmOperation, isEscapedFunction));
                                AppendPath(newPath);
                                found = true;
                            }
                        }
                        else
                        {
                            if (ns is IEdmSingleton)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType),
                                    new ODataOperationSegment(edmOperation, isEscapedFunction));
                                AppendPath(newPath);
                                found = true;
                            }
                            else
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataKeySegment(ns.EntityType()),
                                    new ODataTypeCastSegment(bindingEntityType),
                                    new ODataOperationSegment(edmOperation, isEscapedFunction));
                                AppendPath(newPath);
                                found = true;
                            }
                        }
                    }
                }
            }

            return found;
        }

        private bool HasUnsatisfiedDerivedTypeConstraint(
            IEdmVocabularyAnnotatable annotatable,
            IEdmEntityType baseType,
            OpenApiConvertSettings convertSettings)
        {
            return convertSettings.RequireDerivedTypesConstraintForBoundOperations &&
                   !(_model.GetCollection(annotatable, "Org.OData.Validation.V1.DerivedTypeConstraint") ?? Enumerable.Empty<string>())
                       .Any(c => c.Equals(baseType.FullName(), StringComparison.OrdinalIgnoreCase));
        }

        private bool AppendBoundOperationOnDerivedNavigationPropertyPath(
            IEdmOperation edmOperation,
            bool isCollection,
            IEdmEntityType bindingEntityType,
            OpenApiConvertSettings convertSettings)
        {
            bool found = false;
            bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

            foreach (var baseType in bindingEntityType.FindAllBaseTypes())
            {
                if (_allNavigationPropertyPaths.TryGetValue(baseType, out IList<ODataPath> paths))
                {
                    foreach (var path in paths)
                    {
                        if (path.Kind == ODataPathKind.Ref)
                        {
                            continue;
                        }

                        var npSegment = path.Segments.Last(s => s is ODataNavigationPropertySegment)
                                            as ODataNavigationPropertySegment;
                        if (npSegment == null)
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
                            if (!isLastKeySegment && npSegment.NavigationProperty.TargetMultiplicity() ==
                                EdmMultiplicity.Many)
                            {
                                continue;
                            }
                        }

                        if (HasUnsatisfiedDerivedTypeConstraint(
                                npSegment.NavigationProperty as IEdmVocabularyAnnotatable,
                                baseType,
                                convertSettings))
                        {
                            continue;
                        }

                        ODataPath newPath = path.Clone();
                        newPath.Push(new ODataTypeCastSegment(bindingEntityType));
                        newPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction));
                        AppendPath(newPath);
                        found = true;
                    }
                }
            }

            return found;
        }
    }
}
