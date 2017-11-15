//---------------------------------------------------------------------
// <copyright file="EdmHelper.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods for <see cref="IEdmModel"/>.
    /// </summary>
    internal static class EdmModelExtensions
    {
        /// <summary>
        /// Get the Core.Description annotation.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="element">The vocabulary annotatable element.</param>
        /// <returns>null or the description annotation.</returns>
        public static string GetDescription(this IEdmModel model, IEdmVocabularyAnnotatable element)
        {
            if (model == null || element == null)
            {
                return null;
            }

            IEdmVocabularyAnnotation annotation =
                model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(
                    element, CoreVocabularyModel.DescriptionTerm).FirstOrDefault();
            if (annotation != null)
            {
                IEdmStringConstantExpression stringConstant = annotation.Value as IEdmStringConstantExpression;
                if (stringConstant != null)
                {
                    return stringConstant.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the Core.LongDescription annotation.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="element">The vocabulary annotatable element.</param>
        /// <returns>null or the description annotation.</returns>
        public static string GetLongDescription(this IEdmModel model, IEdmVocabularyAnnotatable element)
        {
            if (model == null || element == null)
            {
                return null;
            }

            IEdmVocabularyAnnotation annotation =
                model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(
                    element, CoreVocabularyModel.LongDescriptionTerm).FirstOrDefault();
            if (annotation != null)
            {
                IEdmStringConstantExpression stringConstant = annotation.Value as IEdmStringConstantExpression;
                if (stringConstant != null)
                {
                    return stringConstant.Value;
                }
            }

            return null;
        }
    }
}
