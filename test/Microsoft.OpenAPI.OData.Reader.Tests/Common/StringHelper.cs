//---------------------------------------------------------------------
// <copyright file="StringHelper.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.OpenAPI.Tests
{
    public static class StringHelper
    {
        public static string Replace(this string rawString, string newLine = "\n")
        {
            rawString = rawString.Trim('\n', '\r');
            rawString = rawString.Replace("\r\n", newLine);
            return rawString;
        }
    }
}
