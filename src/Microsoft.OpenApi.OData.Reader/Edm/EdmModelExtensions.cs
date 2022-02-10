// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.Community.V1;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Extension methods for <see cref="IEdmModel"/>
    /// </summary>
    public static class EdmModelExtensions
    {
        /// <summary>
        /// Determines whether the specified operation is UrlEscape function or not.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="operation">The specified operation.</param>
        /// <returns><c>true</c> if the specified operation is UrlEscape function; otherwise, <c>false</c>.</returns>
        public static bool IsUrlEscapeFunction(this IEdmModel model, IEdmOperation operation)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(operation, nameof(operation));

            if (operation.IsAction())
            {
                return false;
            }

            return model.IsUrlEscapeFunction((IEdmFunction)operation);
        }

        /// <summary>
        /// Determines whether the specified function is UrlEscape function or not.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="function">The specified function</param>
        /// <returns><c>true</c> if the specified operation is UrlEscape function; otherwise, <c>false</c>.</returns>
        public static bool IsUrlEscapeFunction(this IEdmModel model, IEdmFunction function)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(function, nameof(function));

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(function,
                CommunityVocabularyModel.UrlEscapeFunctionTerm).FirstOrDefault();
            if (annotation != null)
            {
                if (annotation.Value == null)
                {
                    // If the annotation is applied but a value is not specified then the value is assumed to be true.
                    return true;
                }

                IEdmBooleanConstantExpression tagConstant = annotation.Value as IEdmBooleanConstantExpression;
                if (tagConstant != null)
                {
                    return tagConstant.Value;
                }
            }

            return false;
        }

        /// <summary>
        /// Load all navigation sources into a dictionary.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The dictionary.</returns>
        public static IDictionary<IEdmEntityType, IList<IEdmNavigationSource>> LoadAllNavigationSources(this IEdmModel model)
        {
            var navigationSourceDic = new Dictionary<IEdmEntityType, IList<IEdmNavigationSource>>();
            if (model != null && model.EntityContainer != null)
            {
                Action<IEdmNavigationSource, IDictionary<IEdmEntityType, IList<IEdmNavigationSource>>> action = (ns, dic) =>
                {
                    if (!dic.TryGetValue(ns.EntityType(), out IList<IEdmNavigationSource> value))
                    {
                        value = new List<IEdmNavigationSource>();
                        dic[ns.EntityType()] = value;
                    }

                    value.Add(ns);
                };

                // entity-set
                foreach (var entitySet in model.EntityContainer.EntitySets())
                {
                    action(entitySet, navigationSourceDic);
                }

                // singleton
                foreach (var singelton in model.EntityContainer.Singletons())
                {
                    action(singelton, navigationSourceDic);
                }
            }

            return navigationSourceDic;
        }

        /// <summary>
        /// Find all base types for a given <see cref="IEdmEntityType"/>
        /// </summary>
        /// <param name="entityType">The given entity type.</param>
        /// <returns>All base types or null.</returns>
        public static IEnumerable<IEdmEntityType> FindAllBaseTypes(this IEdmEntityType entityType)
        {
            if (entityType == null)
            {
                yield return null;
            }

            IEdmEntityType current = entityType.BaseEntityType();
            while (current != null)
            {
                yield return current;
                current = current.BaseEntityType();
            }
        }

        /// <summary>
        /// Find all base types for a given <see cref="IEdmComplexType"/>
        /// </summary>
        /// <param name="complexType">The given complex type.</param>
        /// <returns>All base types or null.</returns>
        public static IEnumerable<IEdmComplexType> FindAllBaseTypes(this IEdmComplexType complexType)
        {
            if (complexType == null)
            {
                yield return null;
            }

            IEdmComplexType current = complexType.BaseComplexType();
            while (current != null)
            {
                yield return current;
                current = current.BaseComplexType();
            }
        }

        /// <summary>
        /// Checks if the <paramref name="baseType"/> is assignable to <paramref name="subtype"/>.
        /// In other words, if <paramref name="subtype"/> is a subtype of <paramref name="baseType"/> or not.
        /// </summary>
        /// <param name="baseType">Type of the base type.</param>
        /// <param name="subtype">Type of the sub type.</param>
        /// <returns>true, if the <paramref name="baseType"/> is assignable to <paramref name="subtype"/>. Otherwise returns false.</returns>
        [Obsolete]
        public static bool IsAssignableFrom(this IEdmEntityType baseType, IEdmEntityType subtype)
        {
            Utils.CheckArgumentNull(baseType, nameof(baseType));
            Utils.CheckArgumentNull(subtype, nameof(subtype));

            if (baseType.TypeKind != subtype.TypeKind)
            {
                return false;
            }

            if (subtype.IsEquivalentTo(baseType))
            {
                return true;
            }

            IEdmStructuredType structuredSubType = subtype;
            while (structuredSubType != null)
            {
                if (structuredSubType.IsEquivalentTo(baseType))
                {
                    return true;
                }

                structuredSubType = structuredSubType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Check whether the operation is overload in the model.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="operation">The test operations.</param>
        /// <returns>True/false.</returns>
        public static bool IsOperationOverload(this IEdmModel model, IEdmOperation operation)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(operation, nameof(operation));

            return model.GetAllElements().OfType<IEdmOperation>()
                .Where(o => o.IsBound == operation.IsBound && o.FullName() == operation.FullName() &&
                o.Parameters.First().Type.Definition == operation.Parameters.First().Type.Definition
                ).Count() > 1;
        }

        /// <summary>
        /// Check whether the operaiton import is overload in the model.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="operationImport">The test operations.</param>
        /// <returns>True/false.</returns>
        public static bool IsOperationImportOverload(this IEdmModel model, IEdmOperationImport operationImport)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(operationImport, nameof(operationImport));

            if (model.EntityContainer == null)
            {
                return false;
            }

            return model.EntityContainer.OperationImports()
                .Where(o => o.Operation.IsBound == operationImport.Operation.IsBound && o.Name == operationImport.Name).Count() > 1;
        }

        /// <summary>
        /// Get all of the elements in the model and its referenced models.
        /// </summary>
        /// <returns>All the elements.</returns>
        public static IEnumerable<IEdmSchemaElement> GetAllElements(this IEdmModel model)
        {
            foreach (var element in model.SchemaElements.Where(el =>
                !ODataConstants.StandardNamespaces.Any(std => el.Namespace.StartsWith(std))))
            {
                yield return element;
            }

            foreach (var refModel in model.ReferencedModels)
            {
                foreach (var element in refModel.SchemaElements.Where(el =>
                    !ODataConstants.StandardNamespaces.Any(std => el.Namespace.StartsWith(std))))
                {
                    yield return element;
                }
            }
        }

    }
}
