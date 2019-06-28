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
    public class DeepUpdateSupportTypeTests
    {
        [Fact]
        public void TermAttributeAttachedOnDeepUpdateSupportType()
        {
            // Arrange & Act
            string qualifiedName = Utils.GetTermQualifiedName<DeepUpdateSupportType>();

            // Assert
            Assert.Equal("Org.OData.Capabilities.V1.DeepUpdateSupport", qualifiedName);
        }

        [Fact]
        public void InitializeDeepUpdateSupportTypeWithRecordSuccess()
        {
            // Assert
            IEdmRecordExpression record = new EdmRecordExpression(
                new EdmPropertyConstructor("Supported", new EdmBooleanConstant(false)),
                new EdmPropertyConstructor("ContentIDSupported", new EdmBooleanConstant(true)));

            // Act
            DeepUpdateSupportType deepUpdate = new DeepUpdateSupportType();
            deepUpdate.Initialize(record);

            // Assert
            VerifyDeepUpdateSupportType(deepUpdate);
        }

        [Fact]
        public void InitializeDeepUpdateSupportTypeWorksWithCsdl()
        {
            // Arrange
            string annotation = @"
                <Annotation Term=""Org.OData.Capabilities.V1.DeepUpdateSupport"" >
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
            DeepUpdateSupportType deepUpdate = model.GetRecord<DeepUpdateSupportType>(calendars);

            // Assert
            VerifyDeepUpdateSupportType(deepUpdate);
        }

        private static void VerifyDeepUpdateSupportType(DeepUpdateSupportType deepUpdate)
        {
            Assert.NotNull(deepUpdate);

            Assert.NotNull(deepUpdate.Supported);
            Assert.False(deepUpdate.Supported.Value);

            Assert.NotNull(deepUpdate.ContentIDSupported);
            Assert.True(deepUpdate.ContentIDSupported.Value);
        }
    }
}
