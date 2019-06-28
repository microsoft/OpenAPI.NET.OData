// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class CollectionPropertyRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnCollectionPropertyRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<CollectionPropertyRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.CollectionPropertyRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializeCollectionPropertyRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("CollectionProperty", new EdmPropertyPathExpression("abc/xyz")),
                new EdmPropertyConstructor("FilterFunctions", new EdmCollectionExpression(new EdmStringConstant("div"))),
                new EdmPropertyConstructor("FilterRestrictions", new EdmRecordExpression(new EdmPropertyConstructor("Filterable", new EdmBooleanConstant(true)))),
                new EdmPropertyConstructor("SearchRestrictions", new EdmRecordExpression(new EdmPropertyConstructor("Searchable", new EdmBooleanConstant(false)))),
                new EdmPropertyConstructor("SortRestrictions", new EdmRecordExpression(new EdmPropertyConstructor("Sortable", new EdmBooleanConstant(false)))),
                new EdmPropertyConstructor("TopSupported", new EdmBooleanConstant(true)),
                new EdmPropertyConstructor("Deletable", new EdmBooleanConstant(false))
                // SkipSupported
                // SelectSupport
                // Insertable
                // Updatable
            );

            // Act
            CollectionPropertyRestrictionsType collectionPropertyRestrictions = new CollectionPropertyRestrictionsType();
            collectionPropertyRestrictions.Initialize(record);

            // Assert
            Assert.Null(collectionPropertyRestrictions.SkipSupported);
            Assert.Null(collectionPropertyRestrictions.SelectSupport);
            Assert.Null(collectionPropertyRestrictions.Insertable);
            Assert.Null(collectionPropertyRestrictions.Updatable);

            Assert.Equal("abc/xyz", collectionPropertyRestrictions.CollectionProperty);

            Assert.NotNull(collectionPropertyRestrictions.FilterFunctions);
            string function = Assert.Single(collectionPropertyRestrictions.FilterFunctions);
            Assert.Equal("div", function);

            Assert.NotNull(collectionPropertyRestrictions.FilterRestrictions);
            Assert.NotNull(collectionPropertyRestrictions.FilterRestrictions.Filterable);
            Assert.True(collectionPropertyRestrictions.FilterRestrictions.Filterable.Value);

            Assert.NotNull(collectionPropertyRestrictions.SearchRestrictions);
            Assert.NotNull(collectionPropertyRestrictions.SearchRestrictions.Searchable);
            Assert.False(collectionPropertyRestrictions.SearchRestrictions.Searchable.Value);

            Assert.NotNull(collectionPropertyRestrictions.SortRestrictions);
            Assert.NotNull(collectionPropertyRestrictions.SortRestrictions.Sortable);
            Assert.False(collectionPropertyRestrictions.SortRestrictions.Sortable.Value);

            Assert.NotNull(collectionPropertyRestrictions.TopSupported);
            Assert.True(collectionPropertyRestrictions.TopSupported.Value);

            Assert.NotNull(collectionPropertyRestrictions.Deletable);
            Assert.False(collectionPropertyRestrictions.Deletable.Value);
        }
    }
}
