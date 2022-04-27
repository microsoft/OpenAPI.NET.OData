// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class MediaEntityPutOperationHandlerTests
    {
        private readonly MediaEntityPutOperationHandler _operationalHandler = new MediaEntityPutOperationHandler();

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void CreateMediaEntityPutOperationReturnsCorrectOperation(bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            string qualifiedName = CapabilitiesConstants.AcceptableMediaTypes;
            string annotation = $@"
            <Annotation Term=""{qualifiedName}"" >
              <Collection>
                <String>image/png</String>
                <String>image/jpeg</String>
              </Collection>
            </Annotation>
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""The logo image."" />";

            // Assert
            VerifyMediaEntityPutOperation("", enableOperationId, useHTTPStatusCodeClass2XX);
            VerifyMediaEntityPutOperation(annotation, enableOperationId, useHTTPStatusCodeClass2XX);
        }

        private void VerifyMediaEntityPutOperation(string annotation, bool enableOperationId, bool useHTTPStatusCodeClass2XX)
        {
            // Arrange
            IEdmModel model = MediaEntityGetOperationHandlerTests.GetEdmModel(annotation);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseHTTPStatusCodeClass2XX = useHTTPStatusCodeClass2XX
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(todos);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.StructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType()),
                new ODataStreamPropertySegment(sp.Name));

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.NavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            // Act
            var putOperation = _operationalHandler.CreateOperation(context, path);
            var putOperation2 = _operationalHandler.CreateOperation(context, path2);

            // Assert
            Assert.NotNull(putOperation);
            Assert.NotNull(putOperation2);
            Assert.Equal("Update Logo for Todo in Todos", putOperation.Summary);
            Assert.Equal("Update media content for the navigation property photo in me", putOperation2.Summary);
            Assert.NotNull(putOperation.Tags);
            Assert.NotNull(putOperation2.Tags);

            var tag = Assert.Single(putOperation.Tags);
            var tag2 = Assert.Single(putOperation2.Tags);
            Assert.Equal("Todos.Todo", tag.Name);
            Assert.Equal("me.profilePhoto", tag2.Name);

            Assert.NotNull(putOperation.Responses);
            Assert.NotNull(putOperation2.Responses);
            Assert.Equal(2, putOperation.Responses.Count);
            Assert.Equal(2, putOperation2.Responses.Count);
            var statusCode = useHTTPStatusCodeClass2XX ? Constants.StatusCodeClass2XX : Constants.StatusCode204;
            Assert.Equal(new[] { statusCode, "default" }, putOperation.Responses.Select(r => r.Key));
            Assert.Equal(new[] { statusCode, "default" }, putOperation2.Responses.Select(r => r.Key));

            if (!string.IsNullOrEmpty(annotation))
            {
                Assert.Equal(2, putOperation.RequestBody.Content.Keys.Count);
                Assert.True(putOperation.RequestBody.Content.ContainsKey("image/png"));
                Assert.True(putOperation.RequestBody.Content.ContainsKey("image/jpeg"));
                Assert.Equal("The logo image.", putOperation.Description);

                Assert.Equal(1, putOperation2.RequestBody.Content.Keys.Count);
                Assert.True(putOperation2.RequestBody.Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }
            else
            {
                Assert.Equal(1, putOperation.RequestBody.Content.Keys.Count);
                Assert.Equal(1, putOperation2.RequestBody.Content.Keys.Count);
                Assert.True(putOperation.RequestBody.Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
                Assert.True(putOperation2.RequestBody.Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }

            if (enableOperationId)
            {
                Assert.Equal("Todos.Todo.UpdateLogo", putOperation.OperationId);
                Assert.Equal("me.UpdatePhotoContent", putOperation2.OperationId);
            }
            else
            {
                Assert.Null(putOperation.OperationId);
                Assert.Null(putOperation2.OperationId);
            }
        }
    }
}
