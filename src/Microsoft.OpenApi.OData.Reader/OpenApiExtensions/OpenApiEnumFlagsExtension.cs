// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;

namespace Microsoft.OpenApi.OData.OpenApiExtensions;

/// <inheritdoc />
[Obsolete("This class is deprecated. Use Microsoft.OpenApi.MicrosoftExtensions.OpenApiEnumFlagsExtension instead.")]
public class OpenApiEnumFlagsExtension : Microsoft.OpenApi.MicrosoftExtensions.OpenApiEnumFlagsExtension
{
    /// <summary>
    /// Name of the extension as used in the description.
    /// </summary>
    public new string Name => Microsoft.OpenApi.MicrosoftExtensions.OpenApiEnumFlagsExtension.Name;
}