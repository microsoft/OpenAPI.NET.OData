// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Edm.Tests
{
    [Collection("Vocabulary annotation cache")]
    public class EdmVocabularyAnnotationExtensionsTests
    {
        private static readonly EdmTerm CachedStringTerm = new EdmTerm("NS", "CachedString", EdmPrimitiveTypeKind.String);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AnnotationCachePreservesMergedRecordsAfterAbsentTermLookups(bool queryAbsentTermFirst)
        {
            var model = GetEdmModel(@"<Annotation Term=""Org.OData.Capabilities.V1.ReadRestrictions"">
  <Record>
    <PropertyValue Property=""Description"" String=""List items"" />
  </Record>
</Annotation>
<Annotation Term=""Org.OData.Core.V1.Description"" String=""Items"" />");
            var target = model.EntityContainer;
            if (queryAbsentTermFirst)
            {
                Assert.Null(model.GetRecord<CountRestrictionsType>(target));
            }

            var readRestrictions = model.GetRecord<ReadRestrictionsType>(target);
            Assert.NotNull(readRestrictions);
            var inheritedRestrictions = new ReadRestrictionsType();
            inheritedRestrictions.Initialize(new EdmRecordExpression(
                new EdmPropertyConstructor("CustomQueryOptions", new EdmCollectionExpression(
                    new EdmRecordExpression(new EdmPropertyConstructor("Name", new EdmStringConstant("includeHiddenFolders")))))));
            readRestrictions.MergePropertiesIfNull(inheritedRestrictions);
            Assert.Equal("Items", model.GetString(target, "Org.OData.Core.V1.Description"));

            Assert.Null(model.GetRecord<CountRestrictionsType>(target));
            Assert.Null(model.GetRecord<CountRestrictionsType>(target));
            Assert.Null(model.GetString(target, "NS.UndefinedTerm"));

            var cachedRestrictions = model.GetRecord<ReadRestrictionsType>(target);
            Assert.Same(readRestrictions, cachedRestrictions);
            Assert.Equal("List items", cachedRestrictions.Description);
            Assert.Equal("includeHiddenFolders", Assert.Single(cachedRestrictions.CustomQueryOptions).Name);
            Assert.Equal("Items", model.GetString(target, "Org.OData.Core.V1.Description"));

            var secondModel = GetEdmModel("");
            Assert.Null(secondModel.GetRecord<ReadRestrictionsType>(secondModel.EntityContainer));

            var reloadedRestrictions = model.GetRecord<ReadRestrictionsType>(target);
            Assert.NotSame(readRestrictions, reloadedRestrictions);
            Assert.Equal("List items", reloadedRestrictions.Description);
            Assert.Null(reloadedRestrictions.CustomQueryOptions);
        }

        [Fact]
        public void AnnotationCacheLooksUpAbsentTermOncePerCallForNewAndExistingTargets()
        {
            var target = new ValueEqualAnnotatable(1, 42);
            var model = GetCacheModel();

            Assert.Null(model.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal(1, model.FindDeclaredVocabularyAnnotationsCallCount);
            Assert.Null(model.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal(2, model.FindDeclaredVocabularyAnnotationsCallCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AnnotationCacheHandlesMixedTargetHashCollisions(bool pathFirst)
        {
            var path = CreateTargetPath();
            var other = new ValueEqualAnnotatable(1, path.GetHashCode());
            Assert.Equal(path.GetHashCode(), other.GetHashCode());
            var model = GetCacheModel(
                new EdmVocabularyAnnotation(path, CachedStringTerm, new EdmStringConstant("path")),
                new EdmVocabularyAnnotation(other, CachedStringTerm, new EdmStringConstant("other")));

            IEdmVocabularyAnnotatable first = pathFirst ? path : other;
            IEdmVocabularyAnnotatable second = pathFirst ? other : path;
            Assert.Equal(pathFirst ? "path" : "other", model.GetString(first, CachedStringTerm.FullName()));
            Assert.Equal(pathFirst ? "other" : "path", model.GetString(second, CachedStringTerm.FullName()));
            // Revisit both keys so a stored path is compared with a nonpath in either insertion order.
            Assert.Equal("other", model.GetString(other, CachedStringTerm.FullName()));
            Assert.Equal("path", model.GetString(path, CachedStringTerm.FullName()));
            Assert.Equal(2, model.FindDeclaredVocabularyAnnotationsCallCount);
        }

        [Fact]
        public void AnnotationCacheSharesEquivalentPathsUsingOrdinalEquality()
        {
            var path = CreateTargetPath();
            var equivalentPath = CreateTargetPath();
            var differentPath = CreateTargetPath("items");
            Assert.NotSame(path, equivalentPath);
            Assert.Equal(path.Path, equivalentPath.Path);
            var model = GetCacheModel(
                new EdmVocabularyAnnotation(path, CachedStringTerm, new EdmStringConstant("original")),
                new EdmVocabularyAnnotation(differentPath, CachedStringTerm, new EdmStringConstant("different")));

            Assert.Equal("original", model.GetString(path, CachedStringTerm.FullName()));
            Assert.Equal("original", model.GetString(equivalentPath, CachedStringTerm.FullName()));
            Assert.Equal("different", model.GetString(differentPath, CachedStringTerm.FullName()));
            Assert.Equal(2, model.FindDeclaredVocabularyAnnotationsCallCount);
        }

        [Fact]
        public void AnnotationCachePreservesNonpathValueEqualityAndHashing()
        {
            var target = new ValueEqualAnnotatable(1, 42);
            var equivalentTarget = new ValueEqualAnnotatable(1, 42);
            var differentTarget = new ValueEqualAnnotatable(2, 42);
            var model = GetCacheModel(
                new EdmVocabularyAnnotation(target, CachedStringTerm, new EdmStringConstant("original")),
                new EdmVocabularyAnnotation(differentTarget, CachedStringTerm, new EdmStringConstant("different")));

            Assert.Equal("original", model.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal("original", model.GetString(equivalentTarget, CachedStringTerm.FullName()));
            Assert.Equal("different", model.GetString(differentTarget, CachedStringTerm.FullName()));
            Assert.Equal(2, model.FindDeclaredVocabularyAnnotationsCallCount);
        }

        [Fact]
        public void AnnotationCachePreservesNonpathReferenceEquality()
        {
            var target = new EdmEntityContainer("NS", "Default");
            var differentTarget = new EdmEntityContainer("NS", "Default");
            var model = GetCacheModel(
                new EdmVocabularyAnnotation(target, CachedStringTerm, new EdmStringConstant("original")),
                new EdmVocabularyAnnotation(differentTarget, CachedStringTerm, new EdmStringConstant("different")));

            Assert.Equal("original", model.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal("different", model.GetString(differentTarget, CachedStringTerm.FullName()));
            Assert.Equal("original", model.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal(2, model.FindDeclaredVocabularyAnnotationsCallCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AnnotationCacheInvalidatesBothTargetKindsWhenModelChanges(bool pathTarget)
        {
            IEdmVocabularyAnnotatable target = pathTarget
                ? CreateTargetPath()
                : new ValueEqualAnnotatable(1, 42);
            var firstModel = GetCacheModel(new EdmVocabularyAnnotation(target, CachedStringTerm, new EdmStringConstant("first")));
            var secondModel = GetCacheModel(new EdmVocabularyAnnotation(target, CachedStringTerm, new EdmStringConstant("second")));

            Assert.Equal("first", firstModel.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal("second", secondModel.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal("first", firstModel.GetString(target, CachedStringTerm.FullName()));
            Assert.Equal(2, firstModel.FindDeclaredVocabularyAnnotationsCallCount);
            Assert.Equal(1, secondModel.FindDeclaredVocabularyAnnotationsCallCount);
        }

        private static CountingAnnotationModel GetCacheModel(params IEdmVocabularyAnnotation[] annotations)
        {
            var model = new CountingAnnotationModel(annotations);
            model.AddElement(CachedStringTerm);
            return model;
        }

        // Moq cannot proxy IEdmModel in Microsoft.OData.Edm 9 because it has ReadOnlySpan<char> members.
        private sealed class CountingAnnotationModel(IEdmVocabularyAnnotation[] annotations) : EdmModel
        {
            public int FindDeclaredVocabularyAnnotationsCallCount { get; private set; }

            // Keep the model lookup reference-based to isolate the annotation cache's key comparison.
            public override IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element)
            {
                FindDeclaredVocabularyAnnotationsCallCount++;
                return annotations.Where(a => ReferenceEquals(a.Target, element));
            }
        }

        private static EdmTargetPath CreateTargetPath(string entitySetName = "Items")
        {
            var container = new EdmEntityContainer("NS", "Default");
            var entitySet = container.AddEntitySet(entitySetName, new EdmEntityType("NS", "Item"));
            return new EdmTargetPath(container, entitySet);
        }

        private sealed class ValueEqualAnnotatable : IEdmVocabularyAnnotatable
        {
            private readonly int value;
            private readonly int hashCode;

            public ValueEqualAnnotatable(int value, int hashCode)
            {
                this.value = value;
                this.hashCode = hashCode;
            }

            public override bool Equals(object obj) => obj is ValueEqualAnnotatable other && value == other.value;

            public override int GetHashCode() => hashCode;
        }

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

            System.Action<CountRestrictionsType> verifyCount = count =>
            {
                // Countable
                Assert.NotNull(count.Countable);
                Assert.True(count.Countable.Value);

                // NonCountableProperties
                Assert.NotNull(count.NonCountableProperties);
                Assert.Equal(new[] { "Emails", "mij" }, count.NonCountableProperties);

                // NonCountableNavigationProperties
                Assert.NotNull(count.NonCountableNavigationProperties);
                Assert.Equal(new[] { "RelatedEvents", "abc" }, count.NonCountableNavigationProperties);
            };

            verifyCount(count1);
            verifyCount(count2);
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

    [CollectionDefinition("Vocabulary annotation cache", DisableParallelization = true)]
    public class VocabularyAnnotationCacheCollection
    {
    }
}
