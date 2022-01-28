// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Linq;

namespace Microsoft.OpenApi.OData.Common;

internal static class StringExtensions
{
	internal static string ToFirstCharacterLowerCase(this string input)
            => string.IsNullOrEmpty(input) ? input : $"{char.ToLowerInvariant(input.FirstOrDefault())}{input.Substring(1)}";
}