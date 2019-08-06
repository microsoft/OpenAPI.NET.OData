// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Security.Cryptography;
using System.Text;

namespace Microsoft.OpenApi.OData.Common
{
    internal static class CryptographyExtensions
    {
        /// <summary>
        /// Calculates the SHA256 hash for the given string.
        /// </summary>
        /// <returns>A 64 char long hash.</returns>
        public static string GetHashSHA256(this string input)
        {
            Utils.CheckArgumentNull(input, nameof(input));

            var hasher = new SHA256CryptoServiceProvider();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = hasher.ComputeHash(inputBytes);
            var hash = new StringBuilder();
            foreach (var b in hashBytes)
            {
                hash.Append(string.Format("{0:x2}", b));
            }
            return hash.ToString();
        }
    }
}
