using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm;

public class ODataComplexPropertySegment : ODataSegment
{
	public ODataComplexPropertySegment(IEdmStructuralProperty property)
	{
		Property = property ?? throw Error.ArgumentNull(nameof(property));
	}
	/// <inheritdoc />
	public override IEdmEntityType EntityType => null;
	public override ODataSegmentKind Kind => ODataSegmentKind.ComplexProperty;

	public override string Identifier => Property.Name;

	public IEdmStructuralProperty Property { get; }

	public IEdmComplexType ComplexType => Property.Type.AsComplex().Definition as IEdmComplexType;

	public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
	{
		return new IEdmVocabularyAnnotatable[] { Property, ComplexType }.Union(ComplexType.FindAllBaseTypes());
	}

	public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => Property.Name;
}