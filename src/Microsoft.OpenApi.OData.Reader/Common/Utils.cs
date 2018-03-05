// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Utilities methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Upper the first character of the string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The changed string.</returns>
        public static string UpperFirstChar(string input)
        {
            if (input == null)
            {
                return input;
            }

            char first = Char.ToUpper(input[0]);
            return first + input.Substring(1);
        }

        /// <summary>
        /// Check the input argument whether its value is null or not.
        /// </summary>
        /// <typeparam name="T">The input value type.</typeparam>
        /// <param name="value">The input value</param>
        /// <param name="parameterName">The input parameter name.</param>
        /// <returns>The input value.</returns>
        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// Check the input string null or empty.
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="parameterName">The input parameter name.</param>
        /// <returns>The input value.</returns>
        internal static string CheckArgumentNullOrEmpty(string value, string parameterName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw Error.ArgumentNullOrEmpty(parameterName);
            }

            return value;
        }
    }
}