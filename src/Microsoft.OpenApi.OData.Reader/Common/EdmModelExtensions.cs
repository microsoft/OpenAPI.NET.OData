// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.OpenApi.OData.Common
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
    }
}