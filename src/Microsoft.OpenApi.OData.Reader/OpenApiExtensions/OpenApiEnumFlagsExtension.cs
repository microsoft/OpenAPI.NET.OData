// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Writers;

namespace Microsoft.OpenApi.OData.OpenApiExtensions;

/// <summary>
/// Extension element for OpenAPI to add deprecation information. x-ms-enum-flags
/// </summary>
public class OpenApiEnumFlagsExtension : IOpenApiExtension
{
    /// <summary>
    /// Name of the extension as used in the description.
    /// </summary>
    public string Name => "x-ms-enum-flags";
    /// <summary>
    /// Whether the enum is a flagged enum.
    /// </summary>
    public bool IsFlags { get; set; }
    /// <summary>
    /// The serialization style of the flagged enum.
    /// </summary>
    public string Style { get; set; }
	/// <inheritdoc />
    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        if(writer == null)
            throw new ArgumentNullException(nameof(writer));
        
        writer.WriteStartObject();
        writer.WriteProperty(nameof(IsFlags).ToFirstCharacterLowerCase(), IsFlags);
        writer.WriteProperty(nameof(Style).ToFirstCharacterLowerCase(),Style);
        writer.WriteEndObject();
    } 
}