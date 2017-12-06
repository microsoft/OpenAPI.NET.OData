// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    /// <summary>
    /// Extension methods for the string in test.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Change the input string's line breaks
        /// </summary>
        /// <param name="rawString">The raw input string</param>
        /// <param name="newLine">The new line break.</param>
        /// <returns>The changed string.</returns>
        public static string ChangeLineBreaks(this string rawString, string newLine = "\n")
        {
            Assert.NotNull(rawString);
            rawString = rawString.Trim('\n', '\r');
            rawString = rawString.Replace("\r\n", newLine);
            return rawString;
        }
    }
}
