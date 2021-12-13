using System;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
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
	public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
	{
		throw new System.NotImplementedException();
	}
}