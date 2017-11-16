// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.OpenApi.OData.Tests
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
