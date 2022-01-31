// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Writers;

namespace Microsoft.OpenApi.OData.OpenApiExtensions;

/// <summary>
/// Extension element for OpenAPI to add enum values descriptions.
/// Based of the AutoRest specification https://github.com/Azure/autorest/blob/main/docs/extensions/readme.md#x-ms-enum
/// </summary>
internal class OpenApiEnumValuesDescriptionExtension : IOpenApiExtension
{
	/// <summary>
    /// Name of the extension as used in the description.
    /// </summary>
	public string Name => "x-ms-enum";

	/// <summary>
	/// The of the enum.
	/// </summary>
	public string EnumName { get; set; }

	/// <summary>
	/// Descriptions for the enum symbols, where the value MUST match the enum symbols in the main description
	/// </summary>
	public List<EnumDescription> ValuesDescriptions { get; set; } = new();

	/// <inheritdoc />
	public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
	{
		if(writer == null)
			throw new ArgumentNullException(nameof(writer));
		if((specVersion == OpenApiSpecVersion.OpenApi2_0 || specVersion == OpenApiSpecVersion.OpenApi3_0) &&
			!string.IsNullOrEmpty(EnumName) &&
			ValuesDescriptions.Any())
		{ // when we upgrade to 3.1, we don't need to write this extension as JSON schema will support writing enum values
			writer.WriteStartObject();
			writer.WriteProperty(nameof(Name).ToFirstCharacterLowerCase(), EnumName);
			writer.WriteProperty("modelAsString", false);
			writer.WriteRequiredCollection("values", ValuesDescriptions, (w, x) => {
				w.WriteStartObject();
				w.WriteProperty(nameof(x.Value).ToFirstCharacterLowerCase(), x.Value);
				w.WriteProperty(nameof(x.Description).ToFirstCharacterLowerCase(), x.Description);
				w.WriteProperty(nameof(x.Name).ToFirstCharacterLowerCase(), x.Name);
				w.WriteEndObject();
			});
			writer.WriteEndObject();
		}
	}
}

internal class EnumDescription : IOpenApiElement
{
	/// <summary>
	/// The description for the enum symbol
	/// </summary>
	public string Description { get; set; }
	/// <summary>
	/// The symbol for the enum symbol to use for code-generation
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// The symbol as described in the main enum schema.
	/// </summary>
	public string Value { get; set; }
}
