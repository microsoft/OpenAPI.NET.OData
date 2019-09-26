// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class ODataOperationSegmentTests
    {
        [Fact]
        public void CtorThrowArgumentNullOperation()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("operation", () => new ODataOperationSegment(operation: null));
        }

        [Fact]
        public void CtorSetOperationProperty()
        {
            // Arrange & Act
            IEdmOperation operation = new EdmAction("NS", "MyAction", null);
            var segment = new ODataOperationSegment(operation);

            // Assert
            Assert.Same(operation, segment.Operation);
        }

        [Fact]
        public void GetEntityTypeThrowsNotImplementedException()
        {
            // Arrange & Act
            IEdmOperation operation = new EdmAction("NS", "MyAction", null);
            var segment = new ODataOperationSegment(operation);

            // Assert
            Assert.Throws<NotImplementedException>(() => segment.EntityType);
        }

        [Fact]
        public void KindPropertyReturnsOperationEnumMember()
        {
            // Arrange & Act
            IEdmOperation operation = new EdmAction("NS", "MyAction", null);
            var segment = new ODataOperationSegment(operation);

            // Assert
            Assert.Equal(ODataSegmentKind.Operation, segment.Kind);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetPathItemNameReturnsCorrectActionLiteral(bool unqualifiedCall, bool isBound)
        {
            // Arrange & Act
            EdmAction action = new EdmAction("NS", "MyAction", null, isBound: isBound, entitySetPathExpression: null);
            var segment = new ODataOperationSegment(action);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUnqualifiedCall = unqualifiedCall
            };

            string expected = unqualifiedCall ? "MyAction" : "NS.MyAction";

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        [Theory]
        [InlineData(true, true, "MyFunction(param={param})")]
        [InlineData(true, false, "MyFunction(entity=@entity,param={param})")]
        [InlineData(false, true, "NS.MyFunction(param={param})")]
        [InlineData(false, false, "NS.MyFunction(entity=@entity,param={param})")]
        public void GetPathItemNameReturnsCorrectFunctionLiteral(bool unqualifiedCall, bool isBound, string expected)
        {
            // Arrange & Act
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction boundFunction = BoundFunction("MyFunction", isBound, entityTypeReference);
            boundFunction.AddParameter("param", parameterType);

            var segment = new ODataOperationSegment(boundFunction);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUnqualifiedCall = unqualifiedCall
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        [Theory]
        [InlineData(true, true, "{param}")]
        [InlineData(true, false, "NS.MyFunction(param={param})")]
        [InlineData(false, true, "NS.MyFunction(param={param})")]
        [InlineData(false, false, "NS.MyFunction(param={param})")]
        public void GetPathItemNameReturnsCorrectFunctionLiteralForEscapedFunction(bool isEscapedFunction, bool enableEscapeFunctionCall, string expected)
        {
            // Arrange & Act
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            EdmFunction boundFunction = BoundFunction("MyFunction", true, entityTypeReference);
            boundFunction.AddParameter("param", parameterType);

            var segment = new ODataOperationSegment(boundFunction, isEscapedFunction);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUriEscapeFunctionCall = enableEscapeFunctionCall
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        [Theory]
        [InlineData(true, true, "{param}:")]
        [InlineData(true, false, "NS.MyFunction(param={param})")]
        [InlineData(false, true, "NS.MyFunction(param={param})")]
        [InlineData(false, false, "NS.MyFunction(param={param})")]
        public void GetPathItemNameReturnsCorrectFunctionLiteralForEscapedComposableFunction(bool isEscapedFunction, bool enableEscapeFunctionCall, string expected)
        {
            // Arrange & Act
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            EdmFunction boundFunction = BoundFunction("MyFunction", true, entityTypeReference, true);
            boundFunction.AddParameter("param", parameterType);

            var segment = new ODataOperationSegment(boundFunction, isEscapedFunction);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUriEscapeFunctionCall = enableEscapeFunctionCall
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        private EdmFunction BoundFunction(string funcName,  bool isBound, IEdmTypeReference firstParameterType, bool isComposable = false)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType,
                isBound: isBound, entitySetPathExpression: null, isComposable: isComposable);
            boundFunction.AddParameter("entity", firstParameterType);
            return boundFunction;
        }
    }
}
