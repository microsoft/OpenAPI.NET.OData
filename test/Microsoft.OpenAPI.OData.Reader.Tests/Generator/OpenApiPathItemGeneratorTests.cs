// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.Community.V1;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using Xunit;

namespace Microsoft.OpenApi.OData.Generator.Tests
{
    public class OpenApiPathItemGeneratorTest
    {
        [Fact]
        public void CreatePathItemsThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreatePathItems());
        }

        [Fact]
        public void CreatePathItemsReturnsForEmptyModel()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            ODataContext context = new ODataContext(model);

            // Act
            var pathItems = context.CreatePathItems();

            // Assert
            Assert.NotNull(pathItems);
            Assert.Empty(pathItems);
        }

        [Theory]
        [InlineData(true, 10)]
        [InlineData(false, 22)]
        public void CreatePathItemsReturnsForBasicModel(bool useAnnotationToGeneratePath, int pathCount)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableKeyAsSegment = true,
                UseRestrictionAnnotationsToGenerateComplexPropertyPaths = useAnnotationToGeneratePath
            };
            ODataContext context = new ODataContext(model, settings);

            // Act
            var pathItems = context.CreatePathItems();

            // Assert
            Assert.NotNull(pathItems);
            Assert.Equal(pathCount, pathItems.Count);

            if (useAnnotationToGeneratePath)
            {
                Assert.Contains("/People", pathItems.Keys);
                Assert.Contains("/People/$count", pathItems.Keys);
                Assert.Contains("/People/{UserName}", pathItems.Keys);
                Assert.Contains("/City", pathItems.Keys);
                Assert.Contains("/City/$count", pathItems.Keys);
                Assert.Contains("/City/{Name}", pathItems.Keys);
                Assert.Contains("/CountryOrRegion", pathItems.Keys);
                Assert.Contains("/CountryOrRegion/$count", pathItems.Keys);
                Assert.Contains("/CountryOrRegion/{Name}", pathItems.Keys);
                Assert.Contains("/Me", pathItems.Keys);
            }
            else
            {
                Assert.Contains("/People", pathItems.Keys);
                Assert.Contains("/People/$count", pathItems.Keys);
                Assert.Contains("/People/{UserName}", pathItems.Keys);
                Assert.Contains("/People/{UserName}/Addresses", pathItems.Keys);
                Assert.Contains("/People/{UserName}/Addresses/$count", pathItems.Keys);
                Assert.Contains("/People/{UserName}/HomeAddress", pathItems.Keys);
                Assert.Contains("/People/{UserName}/HomeAddress/City", pathItems.Keys);
                Assert.Contains("/City", pathItems.Keys);
                Assert.Contains("/City/$count", pathItems.Keys);
                Assert.Contains("/City/{Name}", pathItems.Keys);
                Assert.Contains("/CountryOrRegion", pathItems.Keys);
                Assert.Contains("/CountryOrRegion/$count", pathItems.Keys);
                Assert.Contains("/CountryOrRegion/{Name}", pathItems.Keys);
                Assert.Contains("/Me", pathItems.Keys);
                Assert.Contains("/Me/Addresses", pathItems.Keys);
                Assert.Contains("/Me/Addresses/$count", pathItems.Keys);
                Assert.Contains("/Me/HomeAddress", pathItems.Keys);
                Assert.Contains("/Me/HomeAddress/City", pathItems.Keys);
                Assert.Contains("/Me/WorkAddress", pathItems.Keys);
                Assert.Contains("/Me/WorkAddress/City", pathItems.Keys);
            }            
        }

        [Theory]
        [InlineData(true, true, true, "/Customers({ID}):/{param}:")]
        [InlineData(true, true, false, "/Customers({ID}):/{param}")]
        [InlineData(true, false, true, "/Customers({ID})/NS.MyFunction(param='{param}')")]
        [InlineData(true, false, false, "/Customers({ID})/NS.MyFunction(param='{param}')")]
        [InlineData(false, true, true, "/Customers({ID})/NS.MyFunction(param='{param}')")]
        [InlineData(false, true, false, "/Customers({ID})/NS.MyFunction(param='{param}')")]
        [InlineData(false, false, true, "/Customers({ID})/NS.MyFunction(param='{param}')")]
        [InlineData(false, false, false, "/Customers({ID})/NS.MyFunction(param='{param}')")]
        public void CreatePathItemsReturnsForEscapeFunctionModel(bool enableEscaped, bool hasEscapedAnnotation, bool isComposable, string expected)
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);
            EdmFunction function = new EdmFunction("NS", "MyFunction", EdmCoreModel.Instance.GetString(false), true, null, isComposable);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            function.AddParameter("param", EdmCoreModel.Instance.GetString(false));
            model.AddElement(function);
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = new EdmEntitySet(container, "Customers", customer);
            container.AddElement(customers);
            model.AddElement(container);

            if (hasEscapedAnnotation)
            {
                IEdmBooleanConstantExpression booleanConstant = new EdmBooleanConstant(true);
                IEdmTerm term = CommunityVocabularyModel.UrlEscapeFunctionTerm;
                EdmVocabularyAnnotation annotation = new EdmVocabularyAnnotation(function, term, booleanConstant);
                annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
                model.SetVocabularyAnnotation(annotation);
            }

            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableUriEscapeFunctionCall = enableEscaped,
                AddSingleQuotesForStringParameters = true,
            };
            ODataContext context = new ODataContext(model, settings);

            // Act
            var pathItems = context.CreatePathItems();

            // Assert
            Assert.NotNull(pathItems);
            Assert.Equal(4, pathItems.Count);

            Assert.Contains("/Customers", pathItems.Keys);
            Assert.Contains("/Customers/$count", pathItems.Keys);
            Assert.Contains("/Customers({ID})", pathItems.Keys);
            Assert.Contains(expected, pathItems.Keys);
        }
    }
}
