using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Common.Tests
{
    public class HelpersTests
    {
        [Fact]
        public void GetDerivedTypesReferenceSchemaReturnsDerivedTypesReferencesInSchemaIfExist()
        {
            // Arrange
            IEdmModel edmModel = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(edmModel);
            IEdmEntityType entityType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "directoryObject");
            OpenApiSchema schema = null;

            // Act
            schema = Helpers.GetDerivedTypesReferenceSchema(entityType, context.Model);

            // Assert
            Assert.NotNull(schema.OneOf);
            Assert.Equal(16, schema.OneOf.Count);
        }

        [Fact]
        public void GetDerivedTypesReferenceSchemaReturnsNullSchemaIfNotExist()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.GraphBetaModel;
            ODataContext context = new ODataContext(model);
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "administrativeUnit");
            OpenApiSchema schema = null;

            // Act
            schema = Helpers.GetDerivedTypesReferenceSchema(entityType, context.Model);

            // Assert
            Assert.Null(schema);
        }
    }
}
