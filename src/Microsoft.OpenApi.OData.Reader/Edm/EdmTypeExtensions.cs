// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Extension methods for <see cref="IEdmType"/>
    /// </summary>
    public static class EdmTypeExtensions
    {
        /// <summary>
        /// Determines whether a path parameter should be wrapped in quotes based on the type of the parameter.
        /// </summary>
        /// <param name="edmType">The type of the parameter.</param>
        /// <param name="settings">The conversion settings.</param>
        /// <returns>True if the parameter should be wrapped in quotes, false otherwise.</returns>
        public static bool ShouldPathParameterBeQuoted(this IEdmType edmType, OpenApiConvertSettings settings)
        {
            if (edmType == null || settings == null || !settings.AddSingleQuotesForStringParameters)
            {
                return false;
            }

            return edmType.TypeKind switch
            {
                EdmTypeKind.Enum => true,
                EdmTypeKind.Primitive when edmType.IsString() => true,
                _ => false,
            };
        }
    }

}