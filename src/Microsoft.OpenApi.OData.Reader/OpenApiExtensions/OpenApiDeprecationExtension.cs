using System;
using System.Linq;
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
        if(writer == null)
            throw new ArgumentNullException(nameof(writer));

        if(RemovalDate.HasValue || Date.HasValue || !string.IsNullOrEmpty(Version) || !string.IsNullOrEmpty(Description))
        {
            writer.WriteStartObject();

            if(RemovalDate.HasValue)
                writer.WriteProperty(ToFirstCharacterLowerCase(nameof(RemovalDate)), RemovalDate.Value);
            if(Date.HasValue)
                writer.WriteProperty(ToFirstCharacterLowerCase(nameof(Date)), Date.Value);
            if(!string.IsNullOrEmpty(Version))
                writer.WriteProperty(ToFirstCharacterLowerCase(nameof(Version)), Version);
            if(!string.IsNullOrEmpty(Description))
                writer.WriteProperty(ToFirstCharacterLowerCase(nameof(Description)), Description);

            writer.WriteEndObject();
        }
    }
    private static string ToFirstCharacterLowerCase(string input)
            => string.IsNullOrEmpty(input) ? input : $"{char.ToLowerInvariant(input.FirstOrDefault())}{input.Substring(1)}";
}