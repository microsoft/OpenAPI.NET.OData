// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Moq;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiPathsGeneratorTest
    {
        [Fact]
        public void CreatePathsThrowArgumentNullContext()
        {
            // Arrange
            OpenApiDocument openApiDocument = new();
            ODataContext context = null;
            var mockModel = new Mock<IEdmModel>().Object;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.AddPathsToDocument(openApiDocument));
            Assert.Throws<ArgumentNullException>("document", () => new ODataContext(mockModel).AddPathsToDocument(null));
        }

        [Fact]
        public void CreatePathsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            OpenApiDocument openApiDocument = new();
            ODataContext context = new ODataContext(model);

            // Act
            context.AddPathsToDocument(openApiDocument);
            var paths = openApiDocument.Paths;

            // Assert
            Assert.NotNull(paths);
            Assert.Empty(paths);
        }

        [Theory]
        [InlineData(true, 10)]
        [InlineData(false, 22)]
        public void CreatePathsReturnsForBasicModel(bool useAnnotationToGeneratePath, int pathCount)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            OpenApiDocument openApiDocument = new();
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = true,
                RequireRestrictionAnnotationsToGenerateComplexPropertyPaths = useAnnotationToGeneratePath
            };
            ODataContext context = new ODataContext(model, settings);

            // Act
            context.AddPathsToDocument(openApiDocument);
            var paths = openApiDocument.Paths;

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(pathCount, paths.Count);

            if (useAnnotationToGeneratePath)
            {            
                Assert.Contains("/People", paths.Keys);
                Assert.Contains("/People/$count", paths.Keys);
                Assert.Contains("/People/{UserName}", paths.Keys);
                Assert.Contains("/City", paths.Keys);
                Assert.Contains("/City/$count", paths.Keys);
                Assert.Contains("/City/{Name}", paths.Keys);
                Assert.Contains("/CountryOrRegion", paths.Keys);
                Assert.Contains("/CountryOrRegion/$count", paths.Keys);
                Assert.Contains("/CountryOrRegion/{Name}", paths.Keys);
                Assert.Contains("/Me", paths.Keys);
            }
            else
            {
                Assert.Contains("/People", paths.Keys);
                Assert.Contains("/People/$count", paths.Keys);
                Assert.Contains("/People/{UserName}", paths.Keys);
                Assert.Contains("/People/{UserName}/Addresses", paths.Keys);
                Assert.Contains("/People/{UserName}/Addresses/$count", paths.Keys);
                Assert.Contains("/People/{UserName}/HomeAddress", paths.Keys);
                Assert.Contains("/People/{UserName}/HomeAddress/City", paths.Keys);
                Assert.Contains("/City", paths.Keys);
                Assert.Contains("/City/$count", paths.Keys);
                Assert.Contains("/City/{Name}", paths.Keys);
                Assert.Contains("/CountryOrRegion", paths.Keys);
                Assert.Contains("/CountryOrRegion/$count", paths.Keys);
                Assert.Contains("/CountryOrRegion/{Name}", paths.Keys);
                Assert.Contains("/Me", paths.Keys);
                Assert.Contains("/Me/Addresses", paths.Keys);
                Assert.Contains("/Me/Addresses/$count", paths.Keys);
                Assert.Contains("/Me/HomeAddress", paths.Keys);
                Assert.Contains("/Me/HomeAddress/City", paths.Keys);
                Assert.Contains("/Me/WorkAddress", paths.Keys);
                Assert.Contains("/Me/WorkAddress/City", paths.Keys);
            }
        }

        [Theory]
        [InlineData(true, 10)]
        [InlineData(false, 22)]
        public void CreatePathsReturnsForBasicModelWithPrefix(bool useAnnotationToGeneratePath, int pathCount)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            OpenApiDocument openApiDocument = new();
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = true,
                PathPrefix = "some/prefix",
                RequireRestrictionAnnotationsToGenerateComplexPropertyPaths = useAnnotationToGeneratePath
            };
            ODataContext context = new ODataContext(model, settings);

            // Act
            context.AddPathsToDocument(openApiDocument);
            var paths = openApiDocument.Paths;

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(pathCount, paths.Count);

            if (useAnnotationToGeneratePath)
            {
                Assert.Contains("/some/prefix/People", paths.Keys);
                Assert.Contains("/some/prefix/People/$count", paths.Keys);
                Assert.Contains("/some/prefix/People/{UserName}", paths.Keys);
                Assert.Contains("/some/prefix/City", paths.Keys);
                Assert.Contains("/some/prefix/City/$count", paths.Keys);
                Assert.Contains("/some/prefix/City/{Name}", paths.Keys);
                Assert.Contains("/some/prefix/CountryOrRegion", paths.Keys);
                Assert.Contains("/some/prefix/CountryOrRegion/$count", paths.Keys);
                Assert.Contains("/some/prefix/CountryOrRegion/{Name}", paths.Keys);
                Assert.Contains("/some/prefix/Me", paths.Keys);
            }
            else
            {
                Assert.Contains("/some/prefix/People", paths.Keys);
                Assert.Contains("/some/prefix/People/$count", paths.Keys);
                Assert.Contains("/some/prefix/People/{UserName}", paths.Keys);
                Assert.Contains("/some/prefix/People/{UserName}/Addresses", paths.Keys);
                Assert.Contains("/some/prefix/People/{UserName}/Addresses/$count", paths.Keys);
                Assert.Contains("/some/prefix/People/{UserName}/HomeAddress", paths.Keys);
                Assert.Contains("/some/prefix/People/{UserName}/HomeAddress/City", paths.Keys);
                Assert.Contains("/some/prefix/City", paths.Keys);
                Assert.Contains("/some/prefix/City/$count", paths.Keys);
                Assert.Contains("/some/prefix/City/{Name}", paths.Keys);
                Assert.Contains("/some/prefix/CountryOrRegion", paths.Keys);
                Assert.Contains("/some/prefix/CountryOrRegion/$count", paths.Keys);
                Assert.Contains("/some/prefix/CountryOrRegion/{Name}", paths.Keys);
                Assert.Contains("/some/prefix/Me", paths.Keys);
                Assert.Contains("/some/prefix/Me/Addresses", paths.Keys);
                Assert.Contains("/some/prefix/Me/Addresses/$count", paths.Keys);
                Assert.Contains("/some/prefix/Me/HomeAddress", paths.Keys);
                Assert.Contains("/some/prefix/Me/HomeAddress/City", paths.Keys);
                Assert.Contains("/some/prefix/Me/WorkAddress", paths.Keys);
                Assert.Contains("/some/prefix/Me/WorkAddress/City", paths.Keys);
            }            
        }

        [Fact]
        public void CreatePathsReturnsForContractModelWithHierarhicalClass()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.ContractServiceModel;
            OpenApiDocument openApiDocument = new();
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = true,
                EnableUnqualifiedCall = true
            };
            ODataContext context = new ODataContext(model, settings);

            // Act
            context.AddPathsToDocument(openApiDocument);
            var paths = openApiDocument.Paths;

            // Assert
            Assert.NotNull(paths);
            Assert.Equal(5, paths.Count);

            Assert.Contains("/Accounts", paths.Keys);
            Assert.Contains("/Accounts/$count", paths.Keys);
            Assert.Contains("/Accounts/{id}", paths.Keys);
            Assert.Contains("/Accounts/{id}/Attachments()", paths.Keys);
            Assert.Contains("/Accounts/{id}/AttachmentsAdd", paths.Keys);
        }
    }
}
