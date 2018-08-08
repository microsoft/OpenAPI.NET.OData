// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Extension methods for <see cref="IEdmModel"/>
    /// </summary>
    public static class EdmModelExtensions
    {
        /// <summary>
        /// Gets the vocabulary annotation from a target annotatable.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>The annotation or null.</returns>
        public static IEdmVocabularyAnnotation GetVocabularyAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target, string qualifiedName)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(qualifiedName, nameof(qualifiedName));

            IEdmTerm term = model.FindTerm(qualifiedName);
            if (term != null)
            {
                IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
                if (annotation != null)
                {
                    return annotation;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the vocabulary annotation from a target annotatable.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target Annotatable to find annotation</param>
        /// <returns>The annotation or null.</returns>
        public static IEdmVocabularyAnnotation GetVocabularyAnnotation(this IEdmModel model, IEdmVocabularyAnnotatable target, IEdmTerm term)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(target, nameof(target));
            Utils.CheckArgumentNull(term, nameof(term));

            IEdmVocabularyAnnotation annotation = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, term).FirstOrDefault();
            if (annotation != null)
            {
                return annotation;
            }

            return null;
        }

        /// <summary>
        /// Checks if the <paramref name="baseType"/> is assignable to <paramref name="subtype"/>.
        /// In other words, if <paramref name="subtype"/> is a subtype of <paramref name="baseType"/> or not.
        /// </summary>
        /// <param name="baseType">Type of the base type.</param>
        /// <param name="subtype">Type of the sub type.</param>
        /// <returns>true, if the <paramref name="baseType"/> is assignable to <paramref name="subtype"/>. Otherwise returns false.</returns>
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
        /// Check whether the operaiton is overload in the model.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="operation">The test operations.</param>
        /// <returns>True/false.</returns>
        public static bool IsOperationOverload(this IEdmModel model, IEdmOperation operation)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(operation, nameof(operation));

            return model.SchemaElements.OfType<IEdmOperation>()
                .Where(o => o.IsBound == operation.IsBound && o.FullName() == operation.FullName() &&
                o.Parameters.First().Type.Definition == operation.Parameters.First().Type.Definition
                ).Count() > 1;
        }

        public static bool IsOperationOverload(this IEdmModel model, IEdmOperationImport operationImport)
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

        public static bool IsNavigationTypeOverload(this IEdmModel model, IEdmEntityType entityType, IEdmNavigationProperty navigationProperty)
        {
            Utils.CheckArgumentNull(model, nameof(model));
            Utils.CheckArgumentNull(entityType, nameof(entityType));
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            int count = 0;
            IEdmEntityType nvaEntityType = navigationProperty.ToEntityType();
            foreach(var np in entityType.DeclaredNavigationProperties())
            {
                if (np.ToEntityType() == nvaEntityType)
                {
                    count++;
                }
            }

            return count == 0;
        }
    }
}
