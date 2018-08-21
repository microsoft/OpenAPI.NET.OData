// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataOperationImportSegmentTests
    {
        private IEdmOperationImport _operationImport;

        public ODataOperationImportSegmentTests()
        {
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmAction action = new EdmAction("NS", "MyAction", null);
            _operationImport = new EdmActionImport(container, "MyAction", action);
        }

        [Fact]
        public void CtorThrowArgumentNullOperationImport()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("operationImport", () => new ODataOperationImportSegment(operationImport: null));
        }

        [Fact]
        public void CtorSetOperationImportProperty()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_operationImport);

            // Assert
            Assert.Same(_operationImport, segment.OperationImport);
        }

        [Fact]
        public void GetEntityTypeThrowsNotImplementedException()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_operationImport);

            // Assert
            Assert.Throws<NotImplementedException>(() => segment.EntityType);
        }

        [Fact]
        public void KindPropertyReturnsOperationImportEnumMember()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_operationImport);

            // Assert
            Assert.Equal(ODataSegmentKind.OperationImport, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectOperationImportLiteral()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_operationImport);

            // Assert
            Assert.Equal("MyAction", segment.GetPathItemName(new OpenApiConvertSettings()));
        }
    }
}
