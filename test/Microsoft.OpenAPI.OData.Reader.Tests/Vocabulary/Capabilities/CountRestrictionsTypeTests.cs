// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class CountRestrictionsTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnCountRestrictionsType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<CountRestrictionsType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.CountRestrictions", qualifiedName);
        }

        [Fact]
        public void InitializeCountRestrictionsTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Countable", new EdmBooleanConstant(false) ),
                new EdmPropertyConstructor("NonCountableProperties", new EdmCollectionExpression(
                    new EdmPropertyPathExpression("Emails"),
                    new EdmPropertyPathExpression("mij"))),
                new EdmPropertyConstructor("NonCountableNavigationProperties", new EdmCollectionExpression(
                    new EdmNavigationPropertyPathExpression("RelatedEvents"),
                    new EdmNavigationPropertyPathExpression("abc")))
            );

            // Act
            CountRestrictionsType count = new CountRestrictionsType();
            count.Initialize(record);

            // Assert
            VerifyCountRestrictions(count);
        }

        [Fact]
        public void InitializeCountRestrictionsWorksWithCsdl()
        {
            // Arrange
            string countAnnotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.CountRestrictions"" >
                  <Record>
                    <PropertyValue Property=""Countable"" Bool=""false"" />
                    <PropertyValue Property=""NonCountableProperties"">
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

            IEdmModel model = CapabilitiesModelHelper.GetEdmModelSetInline(countAnnotation);
            Assert.NotNull(model); // guard

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");
            Assert.NotNull(calendars); // guard

            // Act
            CountRestrictionsType count = model.GetRecord<CountRestrictionsType>(calendars);

            // Assert
            VerifyCountRestrictions(count);
        }

        private static void VerifyCountRestrictions(CountRestrictionsType count)
        {
            Assert.NotNull(count);

            Assert.NotNull(count.Countable);
            Assert.False(count.Countable.Value);
            Assert.False(count.IsCountable);

            Assert.NotNull(count.NonCountableProperties);
            Assert.Equal(2, count.NonCountableProperties.Count);
            Assert.Equal("Emails|mij", String.Join("|", count.NonCountableProperties));

            Assert.NotNull(count.NonCountableNavigationProperties);
            Assert.Equal(2, count.NonCountableNavigationProperties.Count);
            Assert.Equal("RelatedEvents,abc", String.Join(",", count.NonCountableNavigationProperties));

            Assert.False(count.IsNonCountableProperty("id"));
            Assert.True(count.IsNonCountableProperty("Emails"));
            Assert.True(count.IsNonCountableNavigationProperty("RelatedEvents"));
        }
    }
}
