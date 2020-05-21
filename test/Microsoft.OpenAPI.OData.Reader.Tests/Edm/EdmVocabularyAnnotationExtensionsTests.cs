// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    public class EdmVocabularyAnnotationExtensionsTests
    {
        [Fact]
        public void GetStringWorksForString()
        {
            // Arrange
            string qualifiedName = "Org.OData.Core.V1.ODataVersions";
            string annotation = $@"<Annotation Term=""{qualifiedName}"" String=""9.99,4.01"" />";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            string versions1 = model.GetString(model.EntityContainer, qualifiedName);
            string versions2 = model.GetString(model.EntityContainer, qualifiedName);

            // Assert
            Assert.NotNull(versions1);
            Assert.NotNull(versions2);
            Assert.Same(versions1, versions2);
            Assert.Equal("9.99,4.01", versions1);
            Assert.Equal("9.99,4.01", versions2);
        }

        [Fact]
        public void GetStringWorksForMutlipleModels()
        {
            // Arrange
            string qualifiedName = "Org.OData.Core.V1.ODataVersions";
            string annotation = $@"<Annotation Term=""{qualifiedName}"" String=""9.99,4.01"" />";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            IEdmModel coreModel = EdmCoreModel.Instance;

            // Act & Assert
            string versions1 = model.GetString(model.EntityContainer, qualifiedName);
            Assert.NotNull(versions1);

            string versions2 = coreModel.GetString(model.EntityContainer, qualifiedName);
            Assert.Null(versions2);

            string versions3 = model.GetString(model.EntityContainer, qualifiedName);
            Assert.NotNull(versions3);

            Assert.Equal(versions1, versions3);
        }

        [Fact]
        public void GetRecordWorksForRecord()
        {
            // Arrange
            string qualifiedName = "Org.OData.Capabilities.V1.CountRestrictions";
            string annotation = $@"
<Annotation Term=""{qualifiedName}"" >
  <Record>
    <PropertyValue Property=""Countable"" Bool=""true"" />
    <PropertyValue Property=""NonCountableProperties"" >
      <Collection>
        <PropertyPath>Emails</PropertyPath>
        <PropertyPath>mij</PropertyPath>
      </Collection>
    </PropertyValue>
    <PropertyValue Property=""NonCountableNavigationProperties"" >
      <Collection>
        <NavigationPropertyPath>RelatedEvents</NavigationPropertyPath>
        <NavigationPropertyPath>abc</NavigationPropertyPath>
      </Collection>
    </PropertyValue>
  </Record>
</Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            CountRestrictionsType count1 = model.GetRecord<CountRestrictionsType>(model.EntityContainer, qualifiedName);
            CountRestrictionsType count2 = model.GetRecord<CountRestrictionsType>(model.EntityContainer, qualifiedName);

            // Assert
            Assert.NotNull(count1);
            Assert.NotNull(count2);
            Assert.Same(count1, count2);

            // Countable
            Assert.NotNull(count1.Countable);
            Assert.True(count1.Countable.Value);

            // NonCountableProperties
            Assert.NotNull(count1.NonCountableProperties);
            Assert.Equal(new[] { "Emails", "mij" }, count1.NonCountableProperties);

            // NonCountableNavigationProperties
            Assert.NotNull(count1.NonCountableNavigationProperties);
            Assert.Equal(new[] { "RelatedEvents", "abc" }, count1.NonCountableNavigationProperties);
        }

        [Fact]
        public void GetCollectionWorksForCollectionOfString()
        {
            // Arrange
            string qualifiedName = "Org.OData.Capabilities.V1.SupportedFormats";
            string annotation = $@"
<Annotation Term=""{qualifiedName}"" >
  <Collection>
    <String>abc</String>
    <String>xyz</String>
  </Collection>
</Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            IEnumerable<string> supportedFormats1 = model.GetCollection(model.EntityContainer, qualifiedName);
            IEnumerable<string> supportedFormats2 = model.GetCollection(model.EntityContainer, qualifiedName);

            // Assert
            Assert.NotNull(supportedFormats1);
            Assert.NotNull(supportedFormats2);
            Assert.Equal(supportedFormats1, supportedFormats2);

            Assert.Equal(2, supportedFormats1.Count());
            Assert.Equal(new[] { "abc", "xyz" }, supportedFormats1);
        }

        [Fact]
        public void GetGenericCollectionWorksForCollectionOfGenericRecord()
        {
            // Arrange
            string qualifiedName = "NS.MyCollectionCountRestrictions";
            string annotation = $@"
<Annotation Term=""{qualifiedName}"" >
  <Collection>
    <Record>
      <PropertyValue Property=""Countable"" Bool=""true"" />
      <PropertyValue Property=""NonCountableProperties"" >
        <Collection>
          <PropertyPath>123</PropertyPath>
          <PropertyPath>abc</PropertyPath>
        </Collection>
      </PropertyValue>
      <PropertyValue Property=""NonCountableNavigationProperties"" >
        <Collection>
          <NavigationPropertyPath>234</NavigationPropertyPath>
          <NavigationPropertyPath>xyz</NavigationPropertyPath>
        </Collection>
      </PropertyValue>
    </Record>
    <Record>
      <PropertyValue Property=""Countable"" Bool=""false"" />
      <PropertyValue Property=""NonCountableProperties"" >
        <Collection>
          <PropertyPath>567</PropertyPath>
          <PropertyPath>mij</PropertyPath>
        </Collection>
      </PropertyValue>
      <PropertyValue Property=""NonCountableNavigationProperties"" >
        <Collection>
          <NavigationPropertyPath>789</NavigationPropertyPath>
          <NavigationPropertyPath>rst</NavigationPropertyPath>
        </Collection>
      </PropertyValue>
    </Record>
  </Collection>
</Annotation>";

            IEdmModel model = GetEdmModel(annotation);
            Assert.NotNull(model); // guard

            // Act
            IEnumerable<CountRestrictionsType> counts1 = model.GetCollection<CountRestrictionsType>(model.EntityContainer, qualifiedName);
            IEnumerable<CountRestrictionsType> counts2 = model.GetCollection<CountRestrictionsType>(model.EntityContainer, qualifiedName);

            // Assert
            Assert.NotNull(counts1);
            Assert.NotNull(counts2);

            Assert.Equal(2, counts1.Count());

            foreach (var countItem in new[] { counts1, counts2 })
            {
                CountRestrictionsType count = countItem.First();

                // Countable
                Assert.NotNull(count.Countable);
                Assert.True(count.Countable.Value);

                // NonCountableProperties
                Assert.NotNull(count.NonCountableProperties);
                Assert.Equal(new[] { "123", "abc" }, count.NonCountableProperties);

                // NonCountableNavigationProperties
                Assert.NotNull(count.NonCountableNavigationProperties);
                Assert.Equal(new[] { "234", "xyz" }, count.NonCountableNavigationProperties);

                // #2
                count = countItem.Last();

                // Countable
                Assert.NotNull(count.Countable);
                Assert.False(count.Countable.Value);

                // NonCountableProperties
                Assert.NotNull(count.NonCountableProperties);
                Assert.Equal(new[] { "567", "mij" }, count.NonCountableProperties);

                // NonCountableNavigationProperties
                Assert.NotNull(count.NonCountableNavigationProperties);
                Assert.Equal(new[] { "789", "rst" }, count.NonCountableNavigationProperties);
            }
        }

        private IEdmModel GetEdmModel(string annotation)
        {
            const string template = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""NS"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name =""Default"">
         {0}
      </EntityContainer>
      <Term Name=""MyCollectionCountRestrictions"" Type=""Collection(Capabilities.CountRestrictions)"" Nullable=""false"" AppliesTo=""EntityContainer"" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            string modelText = string.Format(template, annotation);

            IEdmModel model;
            IEnumerable<EdmError> errors;
            bool result = CsdlReader.TryParse(XElement.Parse(modelText).CreateReader(), out model, out errors);
            Assert.True(result);
            return model;
        }
    }
}
