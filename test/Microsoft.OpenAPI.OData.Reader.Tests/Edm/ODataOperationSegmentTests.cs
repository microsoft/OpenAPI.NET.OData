// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
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
        public void GetEntityTypeReturnsNull()
        {
            // Arrange & Act
            IEdmOperation operation = new EdmAction("NS", "MyAction", null);
            var segment = new ODataOperationSegment(operation);

            // Assert
            Assert.Null(segment.EntityType);
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
        [InlineData(true, true, "XY.MyFunction(param={param},param2=@param2)")]
        [InlineData(true, false, "XY.MyFunction(entity={entity},param={param},param2=@param2)")]
        [InlineData(false, true, "NS.XY.MyFunction(param={param},param2=@param2)")]
        [InlineData(false, false, "NS.XY.MyFunction(entity={entity},param={param},param2=@param2)")]
        public void GetPathItemNameReturnsCorrectFunctionLiteral(bool unqualifiedCall, bool isBound, string expected)
        {
            // Arrange & Act
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(new EdmEntityType("NS.XY", "Entity"), false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction boundFunction = BoundFunction("MyFunction", isBound, entityTypeReference,baseNamespace: "NS.XY");
            boundFunction.AddParameter("param", parameterType);
            boundFunction.AddOptionalParameter("param2", parameterType);

            var segment = new ODataOperationSegment(boundFunction);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUnqualifiedCall = unqualifiedCall,
                BaseNamespace = "NS",
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        [Theory]
        [InlineData(true, true, "{param2}")]
        [InlineData(true, false, "NS.MyFunction(param='{param}',param2='@param2')")]
        [InlineData(false, true, "NS.MyFunction(param='{param}',param2='@param2')")]
        [InlineData(false, false, "NS.MyFunction(param='{param}',param2='@param2')")]
        public void GetPathItemNameReturnsCorrectFunctionLiteralForEscapedFunction(bool isEscapedFunction, bool enableEscapeFunctionCall, string expected)
        {
            // Arrange & Act
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            EdmFunction boundFunction = BoundFunction("MyFunction", true, entityTypeReference);
            boundFunction.AddParameter("param", parameterType);
            boundFunction.AddOptionalParameter("param2", parameterType);

            var segment = new ODataOperationSegment(boundFunction, isEscapedFunction);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUriEscapeFunctionCall = enableEscapeFunctionCall,
                AddSingleQuotesForStringParameters = true,
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        [Theory]
        [InlineData(true, true, "{param2}:")]
        [InlineData(true, false, "NS.MyFunction(param='{param}',param2='@param2')")]
        [InlineData(false, true, "NS.MyFunction(param='{param}',param2='@param2')")]
        [InlineData(false, false, "NS.MyFunction(param='{param}',param2='@param2')")]
        public void GetPathItemNameReturnsCorrectFunctionLiteralForEscapedComposableFunction(bool isEscapedFunction, bool enableEscapeFunctionCall, string expected)
        {
            // Arrange & Act
            IEdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(new EdmEntityType("NS", "Entity"), false);
            IEdmTypeReference parameterType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            EdmFunction boundFunction = BoundFunction("MyFunction", true, entityTypeReference, true);
            boundFunction.AddParameter("param", parameterType);
            boundFunction.AddOptionalParameter("param2", parameterType);

            var segment = new ODataOperationSegment(boundFunction, isEscapedFunction);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUriEscapeFunctionCall = enableEscapeFunctionCall,
                AddSingleQuotesForStringParameters = true
            };

            // Assert
            Assert.Equal(expected, segment.GetPathItemName(settings));
        }

        private EdmFunction BoundFunction(
            string funcName,
            bool isBound,
            IEdmTypeReference firstParameterType,
            bool isComposable = false,
            string baseNamespace = "NS")
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction boundFunction = new EdmFunction(baseNamespace, funcName, returnType,
                isBound: isBound, entitySetPathExpression: null, isComposable: isComposable);
            boundFunction.AddParameter("entity", firstParameterType);
            return boundFunction;
        }
    }
}
