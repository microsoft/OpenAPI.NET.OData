// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.OData.Vocabulary;

namespace Microsoft.OpenApi.OData.Common
{
    /// <summary>
    /// Utilities methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Get the term qualified name when using the type of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the term.</typeparam>
        /// <returns>The qualified name.</returns>
        public static string GetTermQualifiedName<T>()
        {
            object[] attributes = typeof(T).GetCustomAttributes(typeof(TermAttribute), false);
            if (attributes == null && attributes.Length == 0)
            {
                return null;
            }

            TermAttribute term = (TermAttribute)attributes[0];
            return term.QualifiedName;
        }

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
        /// Get an unique name.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="set">The input set.</param>
        /// <returns>The changed string.</returns>
        public static string GetUniqueName(string input, HashSet<string> set)
        {
            if (!set.Contains(input))
            {
                set.Add(input);
                return input;
            }

            int index = 1;
            string newInput;
            do
            {
                newInput = input + index.ToString();
                index++;
            }
            while (set.Contains(newInput));

            set.Add(newInput);
            return newInput;
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

        /// <summary>
        /// Lowers the first character of the string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The changed string.</returns>
        internal static string ToFirstCharacterLowerCase(this string input)
            => string.IsNullOrEmpty(input) ? input : $"{char.ToLowerInvariant(input.FirstOrDefault())}{input.Substring(1)}";
    }
}