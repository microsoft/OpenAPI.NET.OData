using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm;

/// <summary>
/// Represents a property of complex type segment.
/// </summary>
public class ODataComplexPropertySegment : ODataSegment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataComplexPropertySegment"/> class.
    /// </summary>
    /// <param name="property">The complex type property.</param>
    public ODataComplexPropertySegment(IEdmStructuralProperty property)
    {
        Property = property ?? throw Error.ArgumentNull(nameof(property));
    }

    /// <inheritdoc />

    public override IEdmEntityType EntityType => null;
    /// <inheritdoc />
    public override ODataSegmentKind Kind => ODataSegmentKind.ComplexProperty;

    /// <inheritdoc />
    public override string Identifier => Property.Name;

    /// <summary>
    /// Gets the complex type property this segment was inserted for.
    /// </summary>
    public IEdmStructuralProperty Property { get; }

    /// <summary>
    /// Gets the type definition of the property this segment was inserted for.
    /// </summary>
    public IEdmComplexType ComplexType => 
        (Property.Type.IsCollection() ? Property.Type.Definition.AsElementType() : Property.Type.AsComplex().Definition) as IEdmComplexType;

    /// <inheritdoc />
    public override IEnumerable<IEdmVocabularyAnnotatable> GetAnnotables()
    {
        return new IEdmVocabularyAnnotatable[] { Property, ComplexType }.Union(ComplexType.FindAllBaseTypes());
    }

    /// <inheritdoc />
    public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters) => Property.Name;
}