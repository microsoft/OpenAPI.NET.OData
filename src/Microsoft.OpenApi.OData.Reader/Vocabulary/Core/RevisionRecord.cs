using System;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.MicrosoftExtensions;

namespace Microsoft.OpenApi.OData.Vocabulary.Core;

/// <summary>
/// Specialized type for Org.OData.Core.V1.Revisions to easily access the additional properties.
/// </summary>
[Term("Org.OData.Core.V1.Revisions")]
internal class RevisionRecord : RevisionType
{
	/// <summary>
	/// The date at which the element was/will be added, modified or deprecated from the service.
	/// </summary>
	public DateTime? RemovalDate { get; private set; }
    /// <summary>
    /// The date at which the element was/will be be added, modified or deprecated from.
    /// </summary>
    public DateTime? Date { get; private set; }
	/// <summary>
	/// Init the <see cref="RevisionRecord"/>.
	/// </summary>
	/// <param name="record">The input record.</param>
	public override void Initialize(IEdmRecordExpression record)
	{
		base.Initialize(record);
		RemovalDate = record.GetDateTime(nameof(RemovalDate));
		Date = record.GetDateTime(nameof(Date));
	}

    /// <summary>
    /// Gets a <see cref="OpenApiDeprecationExtension"/> from the current annotation.
    /// </summary>
    internal OpenApiDeprecationExtension GetOpenApiExtension()
	{
		return new OpenApiDeprecationExtension
		{
			Date = Date.HasValue ? new DateTimeOffset(Date.Value, TimeSpan.Zero) : default,
			RemovalDate = RemovalDate.HasValue ? new DateTimeOffset(RemovalDate.Value, TimeSpan.Zero) : default,
			Description = Description,
			Version = Version,
		};
	}
}