//---------------------------------------------------------------------
// <copyright file="EdmHelper.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Commons;
using System.Diagnostics;
using System.Linq;

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

        public static string GetOpenApiTypeName(this IEdmTypeReference edmType)
        {
            Debug.Assert(edmType != null);

            return edmType.Definition.GetOpenApiTypeName();
        }

        public static string GetOpenApiTypeName(this IEdmType edmType)
        {
            Debug.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return AnyType.Array.GetDisplayName();

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                case EdmTypeKind.EntityReference:
                    return AnyType.Object.GetDisplayName();

                case EdmTypeKind.Enum:
                    return "string";

                case EdmTypeKind.Primitive:
                    return ((IEdmPrimitiveType)(edmType)).GetOpenApiDataType().GetCommonName();

                default:
                    return AnyType.Null.GetDisplayName();
            }
        }
    }
}
