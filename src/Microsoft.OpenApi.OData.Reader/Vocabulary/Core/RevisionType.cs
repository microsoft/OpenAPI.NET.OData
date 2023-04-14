using System;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Vocabulary.Core;

/// <summary>
/// Complex Type: Org.OData.Core.V1.Revisions
/// </summary>
[Term("Org.OData.Core.V1.Revisions")]
internal class RevisionType : IRecord
{
	/// <summary>
	/// The version this revision was introduced.
	/// </summary>
	public string Version { get; private set; }
	/// <summary>
	/// The kind of the revision
	/// </summary>
	public RevisionKind? Kind { get; private set; }
	/// <summary>
	/// The description of the revision.
	/// </summary>
	public string Description { get; private set; }
	/// <summary>
	/// Init the <see cref="RevisionType"/>.
	/// </summary>
	/// <param name="record">The input record.</param>
	public virtual void Initialize(IEdmRecordExpression record)
	{
		Utils.CheckArgumentNull(record, nameof(record));
		Kind = record.GetEnum<RevisionKind>(nameof(Kind));
		Version = record.GetString(nameof(Version));
		Description = record.GetString(nameof(Description));
	}
}