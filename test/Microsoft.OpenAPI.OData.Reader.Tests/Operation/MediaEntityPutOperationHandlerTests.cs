// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class MediaEntityPutOperationHandlerTests
    {
        private readonly MediaEntityPutOperationHandler _operationalHandler = new MediaEntityPutOperationHandler();

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public void CreateMediaEntityPutOperationReturnsCorrectOperation(bool enableOperationId, bool useSuccessStatusCodeRange)
        {
            // Arrange
            string annotation = $@"
            <Annotation Term=""Org.OData.Core.V1.AcceptableMediaTypes"" >
              <Collection>
                <String>image/png</String>
                <String>image/jpeg</String>
              </Collection>
            </Annotation>
            <Annotation Term=""Org.OData.Core.V1.Description"" String=""The logo image."" />
            <Annotation Term=""Org.OData.Capabilities.V1.UpdateRestrictions"">
              <Record>
                <PropertyValue Property=""CustomQueryOptions"">
                  <Collection>
                    <Record>
                      <PropertyValue Property=""Name"" String=""format"" />
                      <PropertyValue Property=""Description"" String=""Specify the format the item's content should be downloaded as."" />
                      <PropertyValue Property=""Required"" Bool=""false"" />
                    </Record>                
                  </Collection>
                </PropertyValue>
              </Record>
            </Annotation>";

            // Assert
            VerifyMediaEntityPutOperation("", enableOperationId, useSuccessStatusCodeRange);
            VerifyMediaEntityPutOperation(annotation, enableOperationId, useSuccessStatusCodeRange);
        }

        private void VerifyMediaEntityPutOperation(string annotation, bool enableOperationId, bool useSuccessStatusCodeRange)
        {
            // Arrange
            IEdmModel model = MediaEntityGetOperationHandlerTests.GetEdmModel(annotation);
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId,
                UseSuccessStatusCodeRange = useSuccessStatusCodeRange
            };

            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet todos = model.EntityContainer.FindEntitySet("Todos");
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(todos);

            IEdmEntityType todo = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Todo");
            IEdmStructuralProperty sp = todo.StructuralProperties().First(c => c.Name == "Logo");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType),
                new ODataStreamPropertySegment(sp.Name));

            IEdmEntityType user = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "user");
            IEdmNavigationProperty navProperty = user.NavigationProperties().First(c => c.Name == "photo");
            ODataPath path2 = new ODataPath(new ODataNavigationSourceSegment(me),
                new ODataNavigationPropertySegment(navProperty),
                new ODataStreamContentSegment());

            IEdmStructuralProperty sp2 = todo.StructuralProperties().First(c => c.Name == "Content");
            ODataPath path3 = new(new ODataNavigationSourceSegment(todos),
                new ODataKeySegment(todos.EntityType),
                new ODataStreamPropertySegment(sp2.Name));

            // Act
            var putOperation = _operationalHandler.CreateOperation(context, path);
            var putOperation2 = _operationalHandler.CreateOperation(context, path2);
            var putOperation3 = _operationalHandler.CreateOperation(context, path3);

            // Assert
            Assert.NotNull(putOperation);
            Assert.NotNull(putOperation2);
            Assert.NotNull(putOperation3);
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
            Assert.NotNull(putOperation3.Responses);
            
            Assert.Equal(2, putOperation.Responses.Count);
            Assert.Equal(2, putOperation2.Responses.Count);
            Assert.Equal(2, putOperation3.Responses.Count);
            
            var statusCode = (useSuccessStatusCodeRange) ? Constants.StatusCodeClass2XX : Constants.StatusCode204;            
            Assert.Equal(new[] { statusCode, "default" }, putOperation.Responses.Select(r => r.Key));
            Assert.Equal(new[] { statusCode, "default" }, putOperation2.Responses.Select(r => r.Key));
            Assert.Equal(new[] { statusCode, "default" }, putOperation3.Responses.Select(r => r.Key));

            // Test only for stream properties of identifier 'content' 
            if (useSuccessStatusCodeRange)
            {
                var referenceId = putOperation3.Responses[statusCode]?.Content[Constants.ApplicationJsonMediaType]?.Schema?.Reference.Id;
                Assert.NotNull(referenceId);
                Assert.Equal("microsoft.graph.Todo", referenceId);
            }
            else
            {
                Assert.Empty(putOperation3.Responses[statusCode].Content);
            }

            if (!string.IsNullOrEmpty(annotation))
            {
                Assert.Equal(2, putOperation.RequestBody.Content.Keys.Count);
                Assert.True(putOperation.RequestBody.Content.ContainsKey("image/png"));
                Assert.True(putOperation.RequestBody.Content.ContainsKey("image/jpeg"));
                Assert.Equal("The logo image.", putOperation.Description);
                Assert.Equal(2, putOperation.Parameters.Count);
                Assert.NotNull(putOperation.Parameters.FirstOrDefault(x => x.Name.Equals("format")));

                Assert.Single(putOperation2.RequestBody.Content.Keys);
                Assert.True(putOperation2.RequestBody.Content.ContainsKey(Constants.ApplicationOctetStreamMediaType));
            }
            else
            {
                Assert.Single(putOperation.Parameters);
                Assert.Single(putOperation.RequestBody.Content.Keys);
                Assert.Single(putOperation2.RequestBody.Content.Keys);
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

        [Fact]
        public void CreateMediaEntityPropertyPutOperationWithTargetPathAnnotationsReturnsCorrectOperation()
        {
            // Arrange
            IEdmModel model = OData.Tests.EdmModelHelper.TripServiceModel;
            ODataContext context = new(model, new OpenApiConvertSettings());
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = people.EntityType;
            IEdmStructuralProperty property = person.StructuralProperties().First(c => c.Name == "Photo");
            ODataPath path = new(new ODataNavigationSourceSegment(people),
                new ODataKeySegment(person),
                new ODataStreamPropertySegment(property.Name));

            // Act
            var operation = _operationalHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Update photo", operation.Summary);
            Assert.Equal("Update photo of a specific user", operation.Description);

            Assert.NotNull(operation.ExternalDocs);
            Assert.Equal("Find more info here", operation.ExternalDocs.Description);
            Assert.Equal("https://learn.microsoft.com/graph/api/person-update-photo?view=graph-rest-1.0", operation.ExternalDocs.Url.ToString());
        }
    }
}
