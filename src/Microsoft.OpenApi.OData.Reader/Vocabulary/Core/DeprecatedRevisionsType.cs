using System;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.OpenApiExtensions;

namespace Microsoft.OpenApi.OData.Vocabulary.Core;

/// <summary>
/// Specialized type for Org.OData.Core.V1.Revisions to easily access the additional properties.
/// </summary>
[Term("Org.OData.Core.V1.Revisions")]
internal class DeprecatedRevisionsType : RevisionsType
{
	/// <summary>
	/// The date at which the element has been/will be removed entirely from the service.
	/// </summary>
	public DateTime? RemovalDate { get; private set; }
	/// <summary>
	/// The date at which the element has been/will be deprecated.
	/// </summary>
	public DateTime? Date { get; private set; }
	/// <summary>
	/// Init the <see cref="DeprecatedRevisionsType"/>.
	/// </summary>
	/// <param name="record">The input record.</param>
	public override void Initialize(IEdmRecordExpression record)
	{
		base.Initialize(record);
		if (Kind != RevisionKind.Deprecated)
		{
			throw new InvalidOperationException("The kind of the revision must be Deprecated.");
		}
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
			Date = Date,
			RemovalDate = RemovalDate,
			Description = Description,
			Version = Version,
		};
	}
}