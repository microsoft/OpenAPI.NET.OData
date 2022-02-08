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
/// Extension element for OpenAPI to add deprecation information. x-ms-deprecation
/// </summary>
public class OpenApiDeprecationExtension : IOpenApiExtension
{
    /// <summary>
    /// Name of the extension as used in the description.
    /// </summary>
    public string Name => "x-ms-deprecation";
    /// <summary>
    /// The date at which the element has been/will be removed entirely from the service.
    /// </summary>
    public DateTime? RemovalDate { get; set; }
    /// <summary>
    /// The date at which the element has been/will be deprecated.
    /// </summary>
    public DateTime? Date { get; set; }
    /// <summary>
    /// The version this revision was introduced.
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// The description of the revision.
    /// </summary>
    public string Description { get; set; }
	/// <inheritdoc />
    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        if(writer == null)
            throw new ArgumentNullException(nameof(writer));

        if(RemovalDate.HasValue || Date.HasValue || !string.IsNullOrEmpty(Version) || !string.IsNullOrEmpty(Description))
        {
            writer.WriteStartObject();

            if(RemovalDate.HasValue)
                writer.WriteProperty(nameof(RemovalDate).ToFirstCharacterLowerCase(), RemovalDate.Value);
            if(Date.HasValue)
                writer.WriteProperty(nameof(Date).ToFirstCharacterLowerCase(), Date.Value);
            if(!string.IsNullOrEmpty(Version))
                writer.WriteProperty(nameof(Version).ToFirstCharacterLowerCase(), Version);
            if(!string.IsNullOrEmpty(Description))
                writer.WriteProperty(nameof(Description).ToFirstCharacterLowerCase(), Description);

            writer.WriteEndObject();
        }
    } 
}