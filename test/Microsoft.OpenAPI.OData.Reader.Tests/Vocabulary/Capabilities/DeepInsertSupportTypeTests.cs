// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests
{
    public class DeepInsertSupportTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnDeepInsertSupportType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<DeepInsertSupportType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.DeepInsertSupport", qualifiedName);
        }

        [Fact]
        public void InitializeDeepInsertSupportTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Supported", new EdmBooleanConstant(false)),
                new EdmPropertyConstructor("ContentIDSupported", new EdmBooleanConstant(true)));

            // Act
            DeepInsertSupportType deepInsert = new DeepInsertSupportType();
            deepInsert.Initialize(record);

            // Assert
            VerifyDeepInsertSupportType(deepInsert);
        }

        [Fact]
        public void InitializeDeepInsertSupportTypeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.DeepInsertSupport"" >
                  <Record>
                    <PropertyValue Property=""Supported"" Bool=""false"" />
                    <PropertyValue Property=""ContentIDSupported"" Bool=""true"" />
                  </Record>
                </Annotation>";

            IEdmModel model = CapabilitiesModelHelper.GetEdmModelSetInline(annotation);
            Assert.NotNull(model); // guard

            IEdmEntitySet calendars = model.EntityContainer.FindEntitySet("Calendars");
            Assert.NotNull(calendars); // guard

            // Act
            DeepInsertSupportType deepInsert = model.GetRecord<DeepInsertSupportType>(calendars);

            // Assert
            VerifyDeepInsertSupportType(deepInsert);
        }

        private static void VerifyDeepInsertSupportType(DeepInsertSupportType deepInsert)
        {
            Assert.NotNull(deepInsert);

            Assert.NotNull(deepInsert.Supported);
            Assert.False(deepInsert.Supported.Value);

            Assert.NotNull(deepInsert.ContentIDSupported);
            Assert.True(deepInsert.ContentIDSupported.Value);
        }
    }
}
