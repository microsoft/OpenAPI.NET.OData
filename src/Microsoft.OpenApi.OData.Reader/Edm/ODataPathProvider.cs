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
using Microsoft.OpenApi.OData.Common;
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

        private readonly IDictionary<IEdmEntityType, IList<ODataPath>> _dollarCountPaths =
           new Dictionary<IEdmEntityType, IList<ODataPath>>();


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
                   RetrieveNavigationSourcePaths(entitySet, settings);
               }
           }

           // singleton
           foreach (IEdmSingleton singleton in _model.EntityContainer.Singletons())
           {
               if (CanFilter(singleton))
               {
                   RetrieveNavigationSourcePaths(singleton, settings);
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
            Utils.CheckArgumentNull(model, nameof(model));

            _model = model;
            _allNavigationSources = model.LoadAllNavigationSources();
            _allNavigationSourcePaths.Clear();
            _allNavigationPropertyPaths.Clear();
            _allOperationPaths.Clear();
            _dollarCountPaths.Clear();
        }

       private IEnumerable<ODataPath> MergePaths()
       {
           List<ODataPath> allODataPaths = new();
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
            Utils.CheckArgumentNull(path, nameof(path));

            ODataPathKind kind = path.Kind;
            switch(kind)
            {
                case ODataPathKind.ComplexProperty:
                case ODataPathKind.TypeCast:
                case ODataPathKind.DollarCount:
                case ODataPathKind.Entity:
                case ODataPathKind.EntitySet:
                case ODataPathKind.Singleton:
                case ODataPathKind.MediaEntity:
                    if (path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment)
                    {
                        if(!_allNavigationSourcePaths.TryGetValue(navigationSourceSegment.EntityType, out IList<ODataPath> nsList))
                        {
                            nsList = new List<ODataPath>();
                            _allNavigationSourcePaths[navigationSourceSegment.EntityType] = nsList;
                        }
                        
                        if (kind == ODataPathKind.DollarCount)
                        {                          
                            if (_allOperationPaths.FirstOrDefault(p => DollarCountAndOperationPathsSimilar(p, path)) is not null)
                            {
                                // Don't add a path for $count if a similar count() function path already exists.                                
                                return;
                            }
                            else
                            {
                                if (!_dollarCountPaths.TryGetValue(navigationSourceSegment.EntityType, out IList<ODataPath> dollarPathList))
                                {
                                    dollarPathList = new List<ODataPath>();
                                    _dollarCountPaths[navigationSourceSegment.EntityType] = dollarPathList;
                                }
                                dollarPathList.Add(path);
                            }
                        }
                        
                        nsList.Add(path);
                    }
                    break;

                case ODataPathKind.NavigationProperty:
                case ODataPathKind.Ref:
                    ODataNavigationPropertySegment navigationPropertySegment = path.OfType<ODataNavigationPropertySegment>().Last();

                    if (!_allNavigationPropertyPaths.TryGetValue(navigationPropertySegment.EntityType, out IList<ODataPath> npList))
                    {
                        npList = new List<ODataPath>();
                        _allNavigationPropertyPaths[navigationPropertySegment.EntityType] = npList;
                    }

                    npList.Add(path);
                    break;

                case ODataPathKind.Operation:
                case ODataPathKind.OperationImport:
                    if (kind == ODataPathKind.Operation)
                    {
                        foreach (var kvp in _dollarCountPaths)
                        {
                            if (kvp.Value.FirstOrDefault(p => DollarCountAndOperationPathsSimilar(p, path)) is ODataPath dollarCountPath &&
                                _allNavigationSourcePaths.TryGetValue(kvp.Key, out IList<ODataPath> dollarPathList))
                            {
                                dollarPathList.Remove(dollarCountPath);
                                break;
                            }
                        }
                    }

                    _allOperationPaths.Add(path);
                    break;

                default:
                    return;
            }

            bool DollarCountAndOperationPathsSimilar(ODataPath path1, ODataPath path2)
            {
                if ((path1.Kind == ODataPathKind.DollarCount && 
                    path2.Kind == ODataPathKind.Operation && path2.LastSegment.Identifier.Equals(Constants.CountSegmentIdentifier, StringComparison.OrdinalIgnoreCase)) ||
                    (path2.Kind == ODataPathKind.DollarCount &&
                    path1.Kind == ODataPathKind.Operation && path1.LastSegment.Identifier.Equals(Constants.CountSegmentIdentifier, StringComparison.OrdinalIgnoreCase)))
                {
                    return GetModifiedPathItemName(path1)?.Equals(GetModifiedPathItemName(path2), StringComparison.OrdinalIgnoreCase) ?? false;
                }

                return false;                
            }

            string GetModifiedPathItemName(ODataPath path)
            {
                if (!path.Any()) return null;

                IEnumerable<ODataSegment> modifiedSegments = path.Take(path.Count - 1);
                ODataPath modifiedPath = new(modifiedSegments);
                return modifiedPath.GetPathItemName();
            }
        }

        /// <summary>
        /// Retrieve the paths for <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="convertSettings">The settings for the current conversion.</param>
        private void RetrieveNavigationSourcePaths(IEdmNavigationSource navigationSource, OpenApiConvertSettings convertSettings)
        {
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));

            // navigation source itself
            ODataPath path = new(new ODataNavigationSourceSegment(navigationSource));
            AppendPath(path.Clone());

            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType;
            CountRestrictionsType count = null;
            bool? indexableByKey = false;

            // for entity set, create a path with key and a $count path
            if (entitySet != null)
            {
                string targetPath = path.GetTargetPath(_model);
                count = _model.GetRecord<CountRestrictionsType>(targetPath, CapabilitiesConstants.CountRestrictions)
                    ?? _model.GetRecord<CountRestrictionsType>(entitySet, CapabilitiesConstants.CountRestrictions);
                if(count?.Countable ?? true) // ~/entitySet/$count
                    CreateCountPath(path, convertSettings);

                CreateTypeCastPaths(path, convertSettings, entityType, entitySet, true); // ~/entitySet/subType

                if (convertSettings.AddAlternateKeyPaths)
                    CreateAlternateKeyPath(path, entityType); //~/entitySet/{alternateKeyId}

                indexableByKey = _model.GetBoolean(targetPath, CapabilitiesConstants.IndexableByKey)
                    ?? _model.GetBoolean(entitySet, CapabilitiesConstants.IndexableByKey);

                if (indexableByKey ?? true)
                {
                    path.Push(new ODataKeySegment(entityType)); // ~/entitySet/{id}
                    AppendPath(path.Clone());
                    CreateTypeCastPaths(path, convertSettings, entityType, entitySet, false); // ~/entitySet/{id}/subType
                }
            }
            else if (navigationSource is IEdmSingleton singleton)
            { // ~/singleton/subType
                CreateTypeCastPaths(path, convertSettings, entityType, singleton, false);
            }

            // media entity
            RetrieveMediaEntityStreamPaths(entityType, path);

            // properties of type complex
            RetrieveComplexPropertyPaths(entityType, path, convertSettings);

            // navigation property
            foreach (IEdmNavigationProperty np in entityType.NavigationProperties())
            {
                if (CanFilter(np))
                {
                    RetrieveNavigationPropertyPaths(np, count, path, convertSettings);
                }
            }

            if (entitySet != null && (indexableByKey ?? true))
            {
                path.Pop(); // end of entity
            }

            path.Pop(); // end of navigation source.
            Debug.Assert(path.Any() == false);
        }

        /// <summary>
        /// Retrieves the paths for properties of type complex type from entities
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="convertSettings">The settings for the current conversion.</param>
        private void RetrieveComplexPropertyPaths(IEdmEntityType entityType, ODataPath currentPath, OpenApiConvertSettings convertSettings)
        {
            Utils.CheckArgumentNull(entityType, nameof(entityType));
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));

            if (!convertSettings.EnableNavigationPropertyPath) return;

            foreach (IEdmStructuralProperty sp in entityType.StructuralProperties()
                                                    .Where(x => x.Type.IsComplex() ||
                                                            x.Type.IsCollection() && x.Type.Definition.AsElementType() is IEdmComplexType))
            {
                currentPath.Push(new ODataComplexPropertySegment(sp));
                var targetPath = currentPath.GetTargetPath(_model);
                if (!ShouldCreateComplexPropertyPaths(sp, targetPath, convertSettings))
                {
                    currentPath.Pop();
                    continue;
                }
                AppendPath(currentPath.Clone());

                if (sp.Type.IsCollection())
                {
                    CreateTypeCastPaths(currentPath, convertSettings, sp.Type.Definition.AsElementType() as IEdmComplexType, sp, true);
                    var isCountable = _model.GetRecord<CountRestrictionsType>(targetPath, CapabilitiesConstants.CountRestrictions)?.IsCountable
                        ?? _model.GetRecord<CountRestrictionsType>(sp, CapabilitiesConstants.CountRestrictions)?.IsCountable
                        ?? true;
                    if (isCountable)
                        CreateCountPath(currentPath, convertSettings);
                }
                else
                {
                    var complexType = sp.Type.AsComplex().ComplexDefinition();

                    CreateTypeCastPaths(currentPath, convertSettings, complexType, sp, false);

                    // Append navigation property paths for this complex property
                    RetrieveComplexTypeNavigationPropertyPaths(complexType, currentPath, convertSettings);

                    // Traverse this complex property to rerieve nested navigation property paths
                    TraverseComplexProperty(sp, currentPath, convertSettings);
                }

                currentPath.Pop();
            }
        }

        /// <summary>
        /// Retrieves navigation property paths for complex types.
        /// </summary>
        /// <param name="complexType">The target complex type.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="convertSettings">The convert settings.</param>
        private bool RetrieveComplexTypeNavigationPropertyPaths(IEdmComplexType complexType, ODataPath currentPath, OpenApiConvertSettings convertSettings)
        {
            Utils.CheckArgumentNull(complexType, nameof(complexType));
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));

            var navigationProperties = complexType
                                        .DeclaredNavigationProperties()
                                        .Union(complexType
                                                        .FindAllBaseTypes()
                                                        .SelectMany(x => x.DeclaredNavigationProperties()))
                                        .Distinct()
                                        .Where(CanFilter);

            if (!navigationProperties.Any()) return false;

            foreach (var np in navigationProperties)
            {
                var targetPath = currentPath.GetTargetPath(_model);
                var count = _model.GetRecord<CountRestrictionsType>(targetPath, CapabilitiesConstants.CountRestrictions)
                    ?? _model.GetRecord<CountRestrictionsType>(np, CapabilitiesConstants.CountRestrictions);
                RetrieveNavigationPropertyPaths(np, count, currentPath, convertSettings);
            }

            return true;
        }


        /// <summary>
        /// Traverses a complex property to generate navigation property paths within nested complex properties.
        /// </summary>
        /// <param name="structuralProperty">The target complex property.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="convertSettings">The convert settings.</param>
        private void TraverseComplexProperty(IEdmStructuralProperty structuralProperty, ODataPath currentPath, OpenApiConvertSettings convertSettings)
        {
            Utils.CheckArgumentNull(structuralProperty, nameof(structuralProperty));
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));

            var complexType = structuralProperty.Type.AsComplex().ComplexDefinition();
            Debug.Assert(complexType != null);

            foreach (IEdmStructuralProperty sp in complexType.DeclaredStructuralProperties()
                                                    .Where(x => x.Type.IsComplex() ||
                                                            x.Type.IsCollection() && x.Type.Definition.AsElementType() is IEdmComplexType))
            {
                currentPath.Push(new ODataComplexPropertySegment(sp));

                var spComplexType = sp.Type.AsComplex().ComplexDefinition();

                if (!RetrieveComplexTypeNavigationPropertyPaths(spComplexType, currentPath, convertSettings))
                {
                    TraverseComplexProperty(sp, currentPath, convertSettings);
                }

                currentPath.Pop();
            }
        }

        /// <summary>
        /// Evaluates whether or not to create paths for complex properties.
        /// </summary>
        /// <param name="complexProperty">The target complex property.</param>
        /// <param name="targetPath">The annotation target path for the complex property.</param>
        /// <param name="convertSettings">The settings for the current conversion.</param>
        /// <returns>true or false.</returns>
        private bool ShouldCreateComplexPropertyPaths(IEdmStructuralProperty complexProperty, string targetPath, OpenApiConvertSettings convertSettings)
        {
            Utils.CheckArgumentNull(complexProperty, nameof(complexProperty)); 
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));

            if (!convertSettings.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths)
                return true;

            bool isReadable = _model.GetRecord<ReadRestrictionsType>(targetPath, CapabilitiesConstants.ReadRestrictions)?.Readable 
                ?? _model.GetRecord<ReadRestrictionsType>(complexProperty, CapabilitiesConstants.ReadRestrictions)?.Readable 
                ?? false;
            bool isUpdatable = _model.GetRecord<UpdateRestrictionsType>(targetPath, CapabilitiesConstants.UpdateRestrictions)?.Updatable 
                ??_model.GetRecord<UpdateRestrictionsType>(complexProperty, CapabilitiesConstants.UpdateRestrictions)?.Updatable 
                ?? false;
            bool isInsertable = _model.GetRecord<InsertRestrictionsType>(targetPath, CapabilitiesConstants.InsertRestrictions)?.Insertable
                ?? _model.GetRecord<InsertRestrictionsType>(complexProperty, CapabilitiesConstants.InsertRestrictions)?.Insertable 
                ?? false;

            return isReadable || isUpdatable || isInsertable;
        }

        /// <summary>
        /// Retrieves the paths for a media entity stream.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="currentPath">The current OData path.</param>
        private void RetrieveMediaEntityStreamPaths(IEdmEntityType entityType, ODataPath currentPath)
        {
            Utils.CheckArgumentNull(entityType, nameof(entityType));
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));

            bool createValuePath = true;
            foreach (IEdmStructuralProperty sp in entityType.StructuralProperties())
            {
                if (sp.Type.AsPrimitive().IsStream())
                {
                    currentPath.Push(new ODataStreamPropertySegment(sp.Name));
                    AppendPath(currentPath.Clone());
                    currentPath.Pop();
                }

                if (sp.Name.Equals(Constants.Content, StringComparison.OrdinalIgnoreCase))
                {
                    createValuePath = false;
                }
            }

            /* Append a $value segment only if entity (or base type) has stream and
             * does not contain a structural property named Content
             */
            if (createValuePath && (entityType.HasStream || ((entityType.BaseType as IEdmEntityType)?.HasStream ?? false)))
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
        /// <param name="count">The count restrictions.</param>
        /// <param name="currentPath">The current OData path.</param>
        /// <param name="convertSettings">The settings for the current conversion.</param>
        /// <param name="visitedNavigationProperties">A stack that holds the visited navigation properties in the <paramref name="currentPath"/>.</param>
        private void RetrieveNavigationPropertyPaths(
            IEdmNavigationProperty navigationProperty,
            CountRestrictionsType count,
            ODataPath currentPath,
            OpenApiConvertSettings convertSettings,
            Stack<string> visitedNavigationProperties = null)
        {
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));


            if (visitedNavigationProperties == null)
            {
                visitedNavigationProperties = new();
            }

            string navPropFullyQualifiedName = $"{navigationProperty.DeclaringType.FullTypeName()}/{navigationProperty.Name}";

            // Check whether the navigation property has already been navigated in the path.
            if (visitedNavigationProperties.Contains(navPropFullyQualifiedName))
            {
                return;
            }

            // Get the annotatable navigation source for this navigation property.
            IEdmVocabularyAnnotatable annotatableNavigationSource = (currentPath.FirstSegment as ODataNavigationSourceSegment)?.NavigationSource as IEdmVocabularyAnnotatable;                   
            NavigationRestrictionsType navSourceRestrictionType = null;
            NavigationRestrictionsType navPropRestrictionType = null;

            // Get the NavigationRestrictions referenced by this navigation property: Can be defined in the navigation source or in-lined in the navigation property.
            if (annotatableNavigationSource != null)
            {
                navSourceRestrictionType = _model.GetRecord<NavigationRestrictionsType>(annotatableNavigationSource, CapabilitiesConstants.NavigationRestrictions);
                navPropRestrictionType = _model.GetRecord<NavigationRestrictionsType>(navigationProperty, CapabilitiesConstants.NavigationRestrictions);
            }            
            
            NavigationPropertyRestriction restriction = navSourceRestrictionType?.RestrictedProperties?
                .FirstOrDefault(r => r.NavigationProperty == currentPath.NavigationPropertyPath(navigationProperty.Name))
                ?? navPropRestrictionType?.RestrictedProperties?.FirstOrDefault();

            // Check whether the navigation property should be part of the path
            if (!EdmModelHelper.NavigationRestrictionsAllowsNavigability(navSourceRestrictionType, restriction) ||
                !EdmModelHelper.NavigationRestrictionsAllowsNavigability(navPropRestrictionType, restriction))
            {
                return;
            }

            // Whether to expand the navigation property.
            bool shouldExpand = navigationProperty.ContainsTarget;

            // Append a navigation property.
            currentPath.Push(new ODataNavigationPropertySegment(navigationProperty));
            AppendPath(currentPath.Clone());
            visitedNavigationProperties.Push(navPropFullyQualifiedName);

            // For fetching annotations
            var targetPath = currentPath.GetTargetPath(_model);

            // Check whether a collection-valued navigation property should be indexed by key value(s).
            // Find indexability annotation annotated directly via NavigationPropertyRestriction.
            bool? annotatedIndexability = _model.GetBoolean(targetPath, CapabilitiesConstants.IndexableByKey)
                ?? _model.GetBoolean(navigationProperty, CapabilitiesConstants.IndexableByKey);
            bool indexableByKey = true;

            if (restriction?.IndexableByKey != null)
            {
                indexableByKey = (bool)restriction.IndexableByKey;
            }
            else if (annotatedIndexability != null)
            {
                indexableByKey = annotatedIndexability.Value;
            }

            if (indexableByKey)
            {
                IEdmEntityType navEntityType = navigationProperty.ToEntityType();
                var targetsMany = navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many;
                
                if (targetsMany)
                {
                    bool? createCountPath = null;
                    if (count == null)
                    {
                        // First, get the directly annotated restriction annotation of the navigation property
                        count = _model.GetRecord<CountRestrictionsType>(targetPath, CapabilitiesConstants.CountRestrictions)
                            ?? _model.GetRecord<CountRestrictionsType>(navigationProperty, CapabilitiesConstants.CountRestrictions);
                        createCountPath = count?.Countable;
                    }

                    var propertyPath = navigationProperty.GetPartnerPath()?.Path;
                    createCountPath ??= string.IsNullOrEmpty(propertyPath)
                        || (count?.IsNonCountableNavigationProperty(propertyPath) ?? true);

                    if (createCountPath.Value)
                    {
                        // ~/entityset/{key}/collection-valued-Nav/$count
                        CreateCountPath(currentPath, convertSettings);
                    }
                }

                // ~/entityset/{key}/collection-valued-Nav/subtype
                // ~/entityset/{key}/single-valued-Nav/subtype
                CreateTypeCastPaths(currentPath, convertSettings, navEntityType, navigationProperty, targetsMany);

                if ((navSourceRestrictionType?.Referenceable ?? false) ||
                    (navPropRestrictionType?.Referenceable ?? false))
                {
                    // Referenceable navigation properties
                    // Single-Valued: ~/entityset/{key}/single-valued-Nav/$ref
                    // Collection-valued: ~/entityset/{key}/collection-valued-Nav/$ref?$id='{navKey}'
                    CreateRefPath(currentPath);

                    if (targetsMany)
                    {
                        // Collection-valued: DELETE ~/entityset/{key}/collection-valued-Nav/{key}/$ref
                        currentPath.Push(new ODataKeySegment(navEntityType));
                        CreateRefPath(currentPath);

                        CreateTypeCastPaths(currentPath, convertSettings, navEntityType, navigationProperty, false); // ~/entityset/{key}/collection-valued-Nav/{id}/subtype
                    }

                    // Get possible stream paths for the navigation entity type
                    RetrieveMediaEntityStreamPaths(navEntityType, currentPath);

                    // Get the paths for the navigation property entity type properties of type complex
                    RetrieveComplexPropertyPaths(navEntityType, currentPath, convertSettings);
                }
                else
                {
                    // append a navigation property key.
                    if (targetsMany)
                    {
                        CreateAlternateKeyPath(currentPath, navEntityType);

                        currentPath.Push(new ODataKeySegment(navEntityType));
                        AppendPath(currentPath.Clone());

                        CreateTypeCastPaths(currentPath, convertSettings, navEntityType, navigationProperty, false); // ~/entityset/{key}/collection-valued-Nav/{id}/subtype
                    }

                    // Get possible stream paths for the navigation entity type
                    RetrieveMediaEntityStreamPaths(navEntityType, currentPath);

                    // Get the paths for the navigation property entity type properties of type complex
                    RetrieveComplexPropertyPaths(navEntityType, currentPath, convertSettings);

                    if (shouldExpand)
                    {
                        // expand to sub navigation properties
                        foreach (IEdmNavigationProperty subNavProperty in navEntityType.NavigationProperties())
                        {
                            if (CanFilter(subNavProperty))
                            {
                                RetrieveNavigationPropertyPaths(subNavProperty, count, currentPath, convertSettings, visitedNavigationProperties);
                            }
                        }
                    }
                }

                if (targetsMany)
                {
                    currentPath.Pop();
                }                
            }
            currentPath.Pop();
            visitedNavigationProperties.Pop();
        }              

        /// <summary>
        /// Create $ref paths.
        /// </summary>
        /// <param name="currentPath">The current OData path.</param>
        private void CreateRefPath(ODataPath currentPath)
        {
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));

            ODataPath newPath = currentPath.Clone();
            newPath.Push(ODataRefSegment.Instance); // $ref
            AppendPath(newPath);
        }

        /// <summary>
        /// Create $count paths.
        /// </summary>
        /// <param name="currentPath">The current OData path.</param>
        /// <param name="convertSettings">The settings for the current conversion.</param>
        private void CreateCountPath(ODataPath currentPath, OpenApiConvertSettings convertSettings)
        {
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));

            if(!convertSettings.EnableDollarCountPath) 
                return;

            var countPath = currentPath.Clone();
            countPath.Push(ODataDollarCountSegment.Instance);
            AppendPath(countPath);
        }

        /// <summary>
        /// Create path with alternate key
        /// </summary>
        /// <param name="currentPath">The current OData path.</param>
        /// <param name="entityType">The entityType with alternate keys</param>
        private void CreateAlternateKeyPath(ODataPath currentPath, IEdmEntityType entityType)
        {
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(entityType, nameof(entityType));

            IEnumerable<IDictionary<string, IEdmProperty>> alternateKeys = _model.GetAlternateKeysAnnotation(entityType);
            foreach (var keyDict in alternateKeys)
            {
                ODataPath keyPath = currentPath.Clone();
                IDictionary<string, string> keyMappings = keyDict.ToDictionary(static k => k.Key, static v => v.Value.Name);
                ODataKeySegment keySegment = new(entityType, keyMappings)
                {
                    IsAlternateKey = true
                };
                keyPath.Push(keySegment);
                AppendPath(keyPath);
            }
        }

        /// <summary>
        /// Create OData type cast paths.
        /// </summary>
        /// <param name="currentPath">The current OData path.</param>
        /// <param name="convertSettings">The settings for the current conversion.</param>
        /// <param name="structuredType">The type that is being inherited from to which this method will add downcast path segments.</param>
        /// <param name="annotable">The annotable navigation source to read cast annotations from.</param>
        /// <param name="targetsMany">Whether the annotable navigation source targets many entities.</param>
        private void CreateTypeCastPaths(ODataPath currentPath, OpenApiConvertSettings convertSettings, IEdmStructuredType structuredType, IEdmVocabularyAnnotatable annotable, bool targetsMany)
        {
            Utils.CheckArgumentNull(currentPath, nameof(currentPath));
            Utils.CheckArgumentNull(convertSettings, nameof(convertSettings));
            Utils.CheckArgumentNull(structuredType, nameof(structuredType));
            Utils.CheckArgumentNull(annotable, nameof(annotable));

            if(!convertSettings.EnableODataTypeCast)
                return;

            var annotedTypeNames = GetDerivedTypeConstaintTypeNames(annotable);
            
            if(!annotedTypeNames.Any() && convertSettings.RequireDerivedTypesConstraintForODataTypeCastSegments) 
                return; // we don't want to generate any downcast path item if there is no type cast annotation.

            var annotedTypeNamesSet = new HashSet<string>(annotedTypeNames, StringComparer.OrdinalIgnoreCase);

            bool filter(IEdmStructuredType x) =>
                convertSettings.RequireDerivedTypesConstraintForODataTypeCastSegments && annotedTypeNames.Contains(x.FullTypeName()) ||
                !convertSettings.RequireDerivedTypesConstraintForODataTypeCastSegments && (
                    !annotedTypeNames.Any() ||
                    annotedTypeNames.Contains(x.FullTypeName())
                );

            var targetTypes = _model
                                .FindAllDerivedTypes(structuredType)
                                .Where(x => (x.TypeKind == EdmTypeKind.Entity || x.TypeKind == EdmTypeKind.Complex) && filter(x))
                                .OfType<IEdmStructuredType>()
                                .ToArray();

            foreach (var targetType in targetTypes)
            {
                var targetTypeSegment = new ODataTypeCastSegment(targetType, _model);

                if (currentPath.Segments.Any(x => x.Identifier.Equals(targetTypeSegment.Identifier)))
                {
                    // In case we have expanded a derived type's navigation property
                    // and we are in a cyclic loop where the expanded navigation property
                    // has a derived type that has already been added to the path.
                    continue;
                }

                var castPath = currentPath.Clone();
                castPath.Push(targetTypeSegment);
                AppendPath(castPath);
                if (targetsMany)
                {
                    CreateCountPath(castPath, convertSettings);
                }
                else
                {
                    if (convertSettings.GenerateDerivedTypesProperties)
                    {
                        if (annotable is IEdmNavigationProperty navigationProperty && !navigationProperty.ContainsTarget)
                        {
                            continue;
                        }

                        foreach (var declaredNavigationProperty in targetType.NavigationProperties())
                        {
                            RetrieveNavigationPropertyPaths(declaredNavigationProperty, null, castPath, convertSettings);
                        }
                        
                        if (targetType is IEdmEntityType entityType)
                        {
                            RetrieveComplexPropertyPaths(entityType, castPath, convertSettings);
                        }                                                
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve all bounding <see cref="IEdmOperation"/>.
        /// </summary>
        private void RetrieveBoundOperationPaths(OpenApiConvertSettings convertSettings)
        {
            var edmOperations = _model.GetAllElements().OfType<IEdmOperation>().Where(x => x.IsBound).ToArray();
            foreach (var edmOperation in edmOperations)
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

                var allEntitiesForOperation = GetAllEntitiesForOperation(bindingType);

                foreach (var bindingEntityType in allEntitiesForOperation)
                {
                    // 1. Search for corresponding navigation source path
                    AppendBoundOperationOnNavigationSourcePath(edmOperation, isCollection, bindingEntityType, convertSettings);

                    // 2. Search for generated navigation property
                    AppendBoundOperationOnNavigationPropertyPath(edmOperation, isCollection, bindingEntityType);

                    // 3. Search for derived
                    AppendBoundOperationOnDerived(edmOperation, isCollection, bindingEntityType, convertSettings);

                    // 4. Search for derived generated navigation property
                    AppendBoundOperationOnDerivedNavigationPropertyPath(edmOperation, isCollection, bindingEntityType, convertSettings);
                }
            }

            // all operations appended to properties
            // append bound operations to functions
            foreach (var edmOperation in edmOperations)
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

                var allEntitiesForOperation = GetAllEntitiesForOperation(bindingType);

                foreach (var bindingEntityType in allEntitiesForOperation)
                {
                    AppendBoundOperationOnOperationPath(edmOperation, isCollection, bindingEntityType);
                }
            }

            // append navigation properties to functions with return type
            var functionPaths = _allOperationPaths.Where(x => x.LastSegment is ODataOperationSegment operationSegment
                                                    && operationSegment.Operation is IEdmFunction edmFunction
                                                    && edmFunction.IsComposable
                                                    && edmFunction.ReturnType != null
                                                    && edmFunction.ReturnType.Definition is IEdmEntityType returnBindingEntityType);

            foreach( var functionPath in functionPaths)
            {
                if (functionPath.LastSegment is not ODataOperationSegment operationSegment
                                                    || operationSegment.Operation is not IEdmFunction edmFunction
                                                    || !edmFunction.IsComposable
                                                    || edmFunction.ReturnType == null
                                                    || edmFunction.ReturnType.Definition is not IEdmEntityType returnBindingEntityType)
                {
                    continue;
                }

                ODataSegment secondLastSeg = functionPath.ElementAt(functionPath.Count - 2);
                if (convertSettings.ComposableFunctionsExpansionDepth < 2 &&
                            functionPath.LastSegment is ODataOperationSegment &&
                            secondLastSeg is ODataOperationSegment)
                {
                    // Only one level of composable functions expansion allowed
                    continue;
                }

                foreach (var navProperty in returnBindingEntityType.NavigationProperties())
                {
                    /* Get number of segments already appended after the first composable function segment
                     */
                    int composableFuncSegIndex = functionPath.Segments.IndexOf(
                        functionPath.Segments.FirstOrDefault(
                            x => x is ODataOperationSegment operationSegment &&
                            operationSegment.Operation is IEdmFunction edmFunction &&
                            edmFunction.IsComposable));                   
                    int currentDepth = functionPath.Count - composableFuncSegIndex - 1;

                    if (currentDepth < convertSettings.ComposableFunctionsExpansionDepth)
                    {
                        ODataPath newNavigationPath = functionPath.Clone();
                        newNavigationPath.Push(new ODataNavigationPropertySegment(navProperty));
                        AppendPath(newNavigationPath);
                    }                   
                }
            }
        }

        private List<IEdmEntityType> GetAllEntitiesForOperation(IEdmTypeReference bindingType)
        {
            var firstEntityType = bindingType.AsEntity().EntityDefinition();

            bool filter(IEdmNavigationSource z) =>
                z.EntityType != firstEntityType &&
                z.EntityType.FindAllBaseTypes().Contains(firstEntityType);

            return new IEdmEntityType[] { firstEntityType }
                    .Union(_model.EntityContainer.EntitySets()
                            .Where(filter).Select(x => x.EntityType)) //Search all EntitySets
                    .Union(_model.EntityContainer.Singletons()
                            .Where(filter).Select(x => x.EntityType)) //Search all singletons
                    .Distinct()
                    .ToList();
        }

        private static readonly HashSet<ODataPathKind> _oDataPathKindsToSkipForOperationsWhenSingle = new() {
            ODataPathKind.EntitySet,
            ODataPathKind.MediaEntity,
            ODataPathKind.DollarCount,
            ODataPathKind.ComplexProperty,
        };
        private void AppendBoundOperationOnNavigationSourcePath(IEdmOperation edmOperation, bool isCollection, IEdmEntityType bindingEntityType, OpenApiConvertSettings convertSettings)
        {
            if (_allNavigationSourcePaths.TryGetValue(bindingEntityType, out IList<ODataPath> value))
            {
                bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

                foreach (var subPath in value)
                {
                    var lastPathSegment = subPath.LastOrDefault();
                    var secondLastPathSegment = subPath.Count > 1 ? subPath.ElementAt(subPath.Count - 2) : null;
                    if (subPath.Kind == ODataPathKind.TypeCast &&
                        !isCollection &&
                        secondLastPathSegment != null &&
                        secondLastPathSegment is not ODataKeySegment &&
                        (secondLastPathSegment is not ODataNavigationSourceSegment navSource || navSource.NavigationSource is not IEdmSingleton) &&
                        (secondLastPathSegment is not ODataNavigationPropertySegment navProp || navProp.NavigationProperty.Type.IsCollection()))
                    {// we don't want to add operations bound to single elements on type cast segments under collections, only under the key segment, singletons and nav props bound to singles.
                        continue;
                    }
                    else if ((lastPathSegment is not ODataTypeCastSegment castSegment ||
                                castSegment.StructuredType == bindingEntityType ||
                                bindingEntityType.InheritsFrom(castSegment.StructuredType)) && // we don't want to add operations from the parent types under type cast segments because they already are present without the cast
                        ((isCollection && subPath.Kind == ODataPathKind.EntitySet) ||
                            (!isCollection && !_oDataPathKindsToSkipForOperationsWhenSingle.Contains(subPath.Kind))))
                    {
                        if (lastPathSegment is ODataTypeCastSegment && !convertSettings.AppendBoundOperationsOnDerivedTypeCastSegments) continue;
                        if (lastPathSegment is ODataKeySegment segment && segment.IsAlternateKey) continue;

                        var annotatable = (lastPathSegment as ODataNavigationSourceSegment)?.NavigationSource as IEdmVocabularyAnnotatable;
                        annotatable ??= (lastPathSegment as ODataKeySegment)?.EntityType;
                        
                        if (annotatable != null && !EdmModelHelper.IsOperationAllowed(_model, edmOperation, annotatable))
                        {
                            // Check whether the navigation source is allowed to have an operation on the entity type
                            annotatable = (secondLastPathSegment as ODataNavigationSourceSegment)?.NavigationSource as IEdmVocabularyAnnotatable;
                            if (annotatable != null && !EdmModelHelper.IsOperationAllowed(_model, edmOperation, annotatable))
                            {
                                continue;
                            }                          
                        }

                        ODataPath newPath = subPath.Clone();
                        newPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                        AppendPath(newPath);
                    }
                }
            }
        }
        private static readonly HashSet<ODataPathKind> _pathKindToSkipForNavigationProperties = new () {
            ODataPathKind.Ref,
        };
        private void AppendBoundOperationOnNavigationPropertyPath(IEdmOperation edmOperation, bool isCollection, IEdmEntityType bindingEntityType)
        {
            bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

            if (_allNavigationPropertyPaths.TryGetValue(bindingEntityType, out IList<ODataPath> value))
            {
                foreach (var path in value.Where(x => !_pathKindToSkipForNavigationProperties.Contains(x.Kind)))
                {
                    ODataNavigationPropertySegment npSegment = path.Segments.Last(s => s is ODataNavigationPropertySegment) as ODataNavigationPropertySegment;
                    
                    if (!EdmModelHelper.IsOperationAllowed(_model, edmOperation, npSegment.NavigationProperty, npSegment.NavigationProperty.ContainsTarget))
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
                    newPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                    AppendPath(newPath);
                }
            }
        }

        private void AppendBoundOperationOnDerived(
            IEdmOperation edmOperation,
            bool isCollection,
            IEdmEntityType bindingEntityType,
            OpenApiConvertSettings convertSettings)
        {
            if (!convertSettings.AppendBoundOperationsOnDerivedTypeCastSegments) return;
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

                        if (!EdmModelHelper.IsOperationAllowed(_model, edmOperation, ns as IEdmVocabularyAnnotatable))
                        {
                            continue;
                        }

                        if (isCollection)
                        {
                            if (ns is IEdmEntitySet)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType, _model),
                                    new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                                AppendPath(newPath);
                            }
                        }
                        else
                        {
                            if (ns is IEdmSingleton)
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataTypeCastSegment(bindingEntityType, _model),
                                    new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                                AppendPath(newPath);
                            }
                            else
                            {
                                ODataPath newPath = new ODataPath(new ODataNavigationSourceSegment(ns), new ODataKeySegment(ns.EntityType),
                                    new ODataTypeCastSegment(bindingEntityType , _model),
                                    new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                                AppendPath(newPath);
                            }
                        }
                    }
                }
            }
        }

        private bool HasUnsatisfiedDerivedTypeConstraint(
            IEdmVocabularyAnnotatable annotatable,
            IEdmEntityType baseType,
            OpenApiConvertSettings convertSettings)
        {
            return convertSettings.RequireDerivedTypesConstraintForBoundOperations &&
                   !GetDerivedTypeConstaintTypeNames(annotatable)
                       .Any(c => c.Equals(baseType.FullName(), StringComparison.OrdinalIgnoreCase));
        }
        private IEnumerable<string> GetDerivedTypeConstaintTypeNames(IEdmVocabularyAnnotatable annotatable) =>
            _model.GetCollection(annotatable, "Org.OData.Validation.V1.DerivedTypeConstraint") ?? Enumerable.Empty<string>();

        private void AppendBoundOperationOnDerivedNavigationPropertyPath(
            IEdmOperation edmOperation,
            bool isCollection,
            IEdmEntityType bindingEntityType,
            OpenApiConvertSettings convertSettings)
        {
            if (!convertSettings.AppendBoundOperationsOnDerivedTypeCastSegments) return;
            bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

            foreach (var baseType in bindingEntityType.FindAllBaseTypes())
            {
                if (_allNavigationPropertyPaths.TryGetValue(baseType, out IList<ODataPath> paths))
                {
                    foreach (var path in paths.Where(x => !_pathKindToSkipForNavigationProperties.Contains(x.Kind)))
                    {
                        var npSegment = path.Segments.OfType<ODataNavigationPropertySegment>().LastOrDefault();
                        if (npSegment == null)
                        {
                            continue;
                        }

                        if (!EdmModelHelper.IsOperationAllowed(_model, edmOperation, npSegment.NavigationProperty, npSegment.NavigationProperty.ContainsTarget))
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
                                npSegment.NavigationProperty,
                                baseType,
                                convertSettings))
                        {
                            continue;
                        }

                        ODataPath newPath = path.Clone();
                        newPath.Push(new ODataTypeCastSegment(bindingEntityType, _model));
                        newPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                        AppendPath(newPath);
                    }
                }
            }
        }

        private void AppendBoundOperationOnOperationPath(IEdmOperation edmOperation, bool isCollection, IEdmEntityType bindingEntityType)
        {
            bool isEscapedFunction = _model.IsUrlEscapeFunction(edmOperation);

            // only composable functions
            var paths = _allOperationPaths.Where(x => x.LastSegment is ODataOperationSegment operationSegment
                                                    && operationSegment.Operation is IEdmFunction edmFunction
                                                    && edmFunction.IsComposable).ToList();

            foreach (var path in paths)
            {
                if (path.LastSegment is not ODataOperationSegment operationSegment 
                    || (path.Segments.Count > 1 && path.Segments[path.Segments.Count - 2] is ODataOperationSegment)
                    || operationSegment.Operation is not IEdmFunction edmFunction || !edmFunction.IsComposable
                    || edmFunction.ReturnType == null || !edmFunction.ReturnType.Definition.Equals(bindingEntityType)
                    || isCollection
                    || !EdmModelHelper.IsOperationAllowed(_model, edmOperation, operationSegment.Operation, true))
                {
                    continue;
                }

                var segName = path.LastSegment as ODataOperationSegment;
                if (edmOperation.Name.Equals(segName?.Identifier))
                {
                    continue;
                }

                ODataPath newOperationPath = path.Clone();
                newOperationPath.Push(new ODataOperationSegment(edmOperation, isEscapedFunction, _model));
                AppendPath(newOperationPath);
            }
        }
    }
}
