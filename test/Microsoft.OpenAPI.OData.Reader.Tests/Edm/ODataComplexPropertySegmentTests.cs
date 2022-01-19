// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests;
public class ODataComplexPropertySegmentTests
{
    private readonly EdmEntityType _person;
    private readonly EdmStructuralProperty _addressProperty;
    private readonly EdmComplexType _addressComplexType;

    public ODataComplexPropertySegmentTests()
    {
        _person = new EdmEntityType("NS", "Person");
        _addressComplexType = new EdmComplexType("NS", "Address");
        _addressComplexType.AddStructuralProperty("Street", EdmCoreModel.Instance.GetString(false));
        _person.AddKeys(_person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
        _addressProperty = _person.AddStructuralProperty("HomeAddress", new EdmComplexTypeReference(_addressComplexType, false));
    }

    [Fact]
    public void TypeCastSegmentConstructorThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>("property", () => new ODataComplexPropertySegment(null));
    }

    [Fact]
    public void ComplexTypeReturnsPropertyComplexType()
    {
        // Arrange & Act
        var segment = new ODataComplexPropertySegment(_addressProperty);

        // Assert
        Assert.Null(segment.EntityType);
        Assert.Same(_addressComplexType, segment.ComplexType);
    }

    [Fact]
    public void KindPropertyReturnsComplexPropertyEnumMember()
    {
        // Arrange & Act
        var segment = new ODataComplexPropertySegment(_addressProperty);

        // Assert
        Assert.Equal(ODataSegmentKind.ComplexProperty, segment.Kind);
    }

    [Fact]
    public void GetPathItemNameReturnsCorrectPropertyName()
    {
        // Arrange & Act
        var segment = new ODataComplexPropertySegment(_addressProperty);

        // Assert
        Assert.Equal("HomeAddress", segment.GetPathItemName(new OpenApiConvertSettings()));
    }
}
