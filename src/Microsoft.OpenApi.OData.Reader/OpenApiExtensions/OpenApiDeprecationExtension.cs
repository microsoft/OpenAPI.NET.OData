// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;

namespace Microsoft.OpenApi.OData.OpenApiExtensions;

/// <inheritdoc />
[Obsolete("This class is deprecated. Use Microsoft.OpenApi.MicrosoftExtensions.OpenApiDeprecationExtension instead.")]
public class OpenApiDeprecationExtension : Microsoft.OpenApi.MicrosoftExtensions.OpenApiDeprecationExtension
{
    /// <summary>
    /// Name of the extension use in OpenAPI document.
    /// </summary>
    public new string Name => Microsoft.OpenApi.MicrosoftExtensions.OpenApiDeprecationExtension.Name;
    /// <summary>
    /// The date at which the element has been/will be removed entirely from the service.
    /// </summary>
    public new DateTime? RemovalDate { get => base.RemovalDate.HasValue ? base.RemovalDate.Value.DateTime : default; set => base.RemovalDate = value.HasValue ? new DateTimeOffset(value.Value) : default; }
    /// <summary>
    /// The date at which the element has been/will be deprecated.
    /// </summary>
    public new DateTime? Date { get => base.Date.HasValue ? base.Date.Value.DateTime : default; set => base.Date = value.HasValue ? new DateTimeOffset(value.Value) : default; }
}