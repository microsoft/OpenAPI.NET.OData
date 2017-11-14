//---------------------------------------------------------------------
// <copyright file="EdmHelper.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Linq;
using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Commons;

namespace Microsoft.OpenApi.OData
{
    internal static class EdmHelper
    {
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

        public static string GetEntityContainerCoreDescription(this IEdmModel model)
        {
            if (model == null || model.EntityContainer == null)
            {
                return null;
            }

            IEdmVocabularyAnnotation annotation =
                model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(
                    model.EntityContainer, CoreVocabularyModel.DescriptionTerm).FirstOrDefault();
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
