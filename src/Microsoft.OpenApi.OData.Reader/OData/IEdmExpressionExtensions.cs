// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Properties;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Extension methods for <see cref="IEdmExpression"/>.
    /// </summary>
    internal static class IEdmExpressionExtensions
    {
        /// <summary>
        /// Convert an <see cref="IEdmExpression"/> to a <see cref="ODataValue"/>
        /// </summary>
        /// <param name="expression">The <see cref="IEdmExpression"/>.</param>
        /// <returns>The null or <see cref="ODataValue"/>.</returns>
        public static ODataValue Convert(this IEdmExpression expression)
        {
            if (expression == null)
            {
                return null;
            }

            switch (expression.ExpressionKind)
            {
                case EdmExpressionKind.BinaryConstant:
                    IEdmBinaryConstantExpression binaryConstant = (IEdmBinaryConstantExpression)expression;
                    return new ODataPrimitiveValue(binaryConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetBinary(false)
                    };

                case EdmExpressionKind.BooleanConstant:
                    IEdmBooleanConstantExpression booleanConstant = (IEdmBooleanConstantExpression)expression;
                    return new ODataPrimitiveValue(booleanConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetBoolean(false)
                    };

                case EdmExpressionKind.DateTimeOffsetConstant:
                    IEdmDateTimeOffsetConstantExpression dateTimeOffsetConstant = (IEdmDateTimeOffsetConstantExpression)expression;
                    return new ODataPrimitiveValue(dateTimeOffsetConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetDateTimeOffset(false)
                    };

                case EdmExpressionKind.DecimalConstant:
                    IEdmDecimalConstantExpression decimalConstant = (IEdmDecimalConstantExpression)expression;
                    return new ODataPrimitiveValue(decimalConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetDecimal(false)
                    };

                case EdmExpressionKind.FloatingConstant:
                    IEdmFloatingConstantExpression floatConstant = (IEdmFloatingConstantExpression)expression;
                    return new ODataPrimitiveValue(floatConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetDouble(false)
                    };

                case EdmExpressionKind.GuidConstant:
                    IEdmGuidConstantExpression guidConstant = (IEdmGuidConstantExpression)expression;
                    return new ODataPrimitiveValue(guidConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetGuid(false)
                    };

                case EdmExpressionKind.IntegerConstant:
                    IEdmIntegerConstantExpression integerConstant = (IEdmIntegerConstantExpression)expression;
                    return new ODataPrimitiveValue(integerConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetInt64(false)
                    };

                case EdmExpressionKind.StringConstant:
                    IEdmStringConstantExpression stringConstant = (IEdmStringConstantExpression)expression;
                    return new ODataPrimitiveValue(stringConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetString(false)
                    };

                case EdmExpressionKind.DurationConstant:
                    IEdmDurationConstantExpression durationConstant = (IEdmDurationConstantExpression)expression;
                    return new ODataPrimitiveValue(durationConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetDuration(false)
                    };

                case EdmExpressionKind.TimeOfDayConstant:
                    IEdmTimeOfDayConstantExpression timeOfDayConstant = (IEdmTimeOfDayConstantExpression)expression;
                    return new ODataPrimitiveValue(timeOfDayConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetTimeOfDay(false)
                    };

                case EdmExpressionKind.DateConstant:
                    IEdmDateConstantExpression dateConstant = (IEdmDateConstantExpression)expression;
                    return new ODataPrimitiveValue(dateConstant.Value)
                    {
                        TypeReference = EdmCoreModel.Instance.GetDate(false)
                    };

                case EdmExpressionKind.Record:
                    IEdmRecordExpression recordExpression = (IEdmRecordExpression)expression;
                    return new ODataResourceValue
                    {
                        TypeReference = recordExpression.DeclaredType,
                        Properties = recordExpression.Properties.ToDictionary(p => p.Name, p => p.Value.Convert())
                    };

                case EdmExpressionKind.Collection:
                    IEdmCollectionExpression collectionExpression = (IEdmCollectionExpression)expression;
                    ODataCollectValue collectionValue = new ODataCollectValue
                    {
                        TypeReference = collectionExpression.DeclaredType
                    };

                    collectionValue.Elements = collectionExpression.Elements.Select(e => e.Convert()).ToList();
                    return collectionValue;

                case EdmExpressionKind.Path:
                case EdmExpressionKind.PropertyPath:
                case EdmExpressionKind.NavigationPropertyPath:
                case EdmExpressionKind.EnumMember:
                default:
                    throw new NotSupportedException(String.Format(SRResource.NotSupportedEdmExpressionKind, expression.ExpressionKind));
            }
        }
    }
}
