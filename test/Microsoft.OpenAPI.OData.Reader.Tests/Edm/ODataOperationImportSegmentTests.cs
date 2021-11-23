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
        private IEdmOperationImport _actionImport;
        private IEdmOperationImport _functionImport;

        public ODataOperationImportSegmentTests()
        {
            IEdmEntityContainer container = new EdmEntityContainer("NS", "default");
            IEdmAction action = new EdmAction("NS", "MyAction", null);
            _actionImport = new EdmActionImport(container, "MyAction", action);

            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), false, null, false);
            function.AddParameter("firstName", EdmCoreModel.Instance.GetString(false));
            function.AddParameter("lastName", EdmCoreModel.Instance.GetString(false));
            _functionImport = new EdmFunctionImport(container, "MyFunction", function);
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
            var segment = new ODataOperationImportSegment(_actionImport);

            // Assert
            Assert.Same(_actionImport, segment.OperationImport);
        }

        [Fact]
        public void GetEntityTypeThrowsNotImplementedException()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_actionImport);

            // Assert
            Assert.Throws<NotImplementedException>(() => segment.EntityType);
        }

        [Fact]
        public void KindPropertyReturnsOperationImportEnumMember()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_actionImport);

            // Assert
            Assert.Equal(ODataSegmentKind.OperationImport, segment.Kind);
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectActionImportLiteral()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_actionImport);

            // Assert
            Assert.Equal("MyAction", segment.GetPathItemName(new OpenApiConvertSettings()));
        }

        [Fact]
        public void GetPathItemNameReturnsCorrectFunctionImportLiteral()
        {
            // Arrange & Act
            var segment = new ODataOperationImportSegment(_functionImport);

            // Assert
            Assert.Equal("MyFunction(firstName='{firstName}',lastName='{lastName}')",
                segment.GetPathItemName(new OpenApiConvertSettings()));
        }
    }
}
