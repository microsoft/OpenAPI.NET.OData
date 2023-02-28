// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataTypeCastSegmentTests
    {
        private readonly EdmEntityType _person;
        private readonly EdmModel _model;

        public ODataTypeCastSegmentTests()
        {
            _model = new EdmModel();
            _model.SetNamespaceAlias("NS", "N");
            _person = _model.AddEntityType("NS", "Person");
            _person.AddKeys(_person.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false)));
        }

        [Fact]
        public void TypeCastSegmentConstructorThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("structuredType", () => new ODataTypeCastSegment(null, null));
        }

        [Fact]
        public void TypeCastSegmentEntityTypePropertyReturnsSameEntityType()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person, _model);

            // Assert
            Assert.Null(segment.EntityType);
            Assert.Same(_person, segment.StructuredType);
        }

        [Fact]
        public void KindPropertyReturnsTypeCastEnumMember()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person, _model);

            // Assert
            Assert.Equal(ODataSegmentKind.TypeCast, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectTypeCastLiteral()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person,_model);

            // Assert
            Assert.Equal("NS.Person", segment.GetPathItemName(new OpenApiConvertSettings()));
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectTypeCastLiteralAsAliased()
        {
            // Arrange & Act
            var segment = new ODataTypeCastSegment(_person, _model);
            var settings = new OpenApiConvertSettings()
            {
                EnableAliasForTypeCastSegments = true
            };

            // Assert
            Assert.Equal("N.Person", segment.GetPathItemName(settings));
        }
    }
}
