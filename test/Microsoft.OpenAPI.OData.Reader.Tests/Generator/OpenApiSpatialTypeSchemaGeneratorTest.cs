﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests
{
    public class OpenApiSpatialTypeSchemaGeneratorTest
    {
        [Fact]
        public void CreateSpatialSchemasThrowArgumentNullContext()
        {
            // Arrange
            ODataContext context = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => context.CreateSpatialSchemas(new()));
        }

        [Fact]
        public void CreateSpatialSchemasReturnEmptyForCoreModel()
        {
            // Arrange
            ODataContext context = new ODataContext(EdmCoreModel.Instance);

            // Act
            var schemas = context.CreateSpatialSchemas(new());

            // Assert
            Assert.NotNull(schemas);
            Assert.Empty(schemas);
        }

        [Fact]
        public void CreateSpatialSchemasReturnFullSpatialSchemasForModelWithEdmSpatialTypes()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmComplexType complex = new EdmComplexType("NS", "Complex");
            complex.AddStructuralProperty("Location", EdmPrimitiveTypeKind.Geography);
            model.AddElement(complex);

            ODataContext context = new ODataContext(model);

            // Act
            var schemas = context.CreateSpatialSchemas(new());

            // Assert
            Assert.NotNull(schemas);
            Assert.NotEmpty(schemas);
            Assert.Equal(new string[]
            {
                "Edm.Geography",
                "Edm.GeographyPoint",
                "Edm.GeographyLineString",
                "Edm.GeographyPolygon",
                "Edm.GeographyMultiPoint",
                "Edm.GeographyMultiLineString",
                "Edm.GeographyMultiPolygon",
                "Edm.GeographyCollection",

                "Edm.Geometry",
                "Edm.GeometryPoint",
                "Edm.GeometryLineString",
                "Edm.GeometryPolygon",
                "Edm.GeometryMultiPoint",
                "Edm.GeometryMultiLineString",
                "Edm.GeometryMultiPolygon",
                "Edm.GeometryCollection",

                "GeoJSON.position"
            },
            schemas.Select(s => s.Key));
        }

        [Fact]
        public async Task CreateEdmGeographySchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographySchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.Geometry""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyPointSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyPointSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryPoint""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyLineStringSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyLineStringSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryLineString""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyPolygonSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyPolygonSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryPolygon""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyMultiPointSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyMultiPointSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryMultiPoint""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyMultiLineStringSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyMultiLineStringSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryMultiLineString""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyMultiPolygonSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyMultiPolygonSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryMultiPolygon""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeographyCollectionSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeographyCollectionSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""$ref"": ""#/components/schemas/Edm.GeometryCollection""
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeometrySchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometrySchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""type"": ""object"",
  ""oneOf"": [
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryPoint""
    },
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryLineString""
    },
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryPolygon""
    },
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryMultiPoint""
    },
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryMultiLineString""
    },
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryMultiPolygon""
    },
    {
      ""$ref"": ""#/components/schemas/Edm.GeometryCollection""
    }
  ]
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeometryPointSchemaSerializeAsYamlWorks() // test yaml
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryPointSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string yaml = await schema.SerializeAsYamlAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(
                @"required:
  - type
  - coordinates
type: object
properties:
  type:
    enum:
      - Point
    type: string
    default: Point
  coordinates:
    $ref: '#/components/schemas/GeoJSON.position'".ChangeLineBreaks(), yaml);
        }

        [Fact]
        public async Task CreateEdmGeometryLineStringSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryLineStringSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""required"": [
    ""type"",
    ""coordinates""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""type"": {
      ""enum"": [
        ""LineString""
      ]
    },
    ""coordinates"": {
      ""minItems"": 2,
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/components/schemas/GeoJSON.position""
      }
    }
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeometryPolygonSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryPolygonSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""required"": [
    ""type"",
    ""coordinates""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""type"": {
      ""enum"": [
        ""Polygon""
      ]
    },
    ""coordinates"": {
      ""minItems"": 4,
      ""type"": ""array"",
      ""items"": {
        ""type"": ""array"",
        ""items"": {
          ""$ref"": ""#/components/schemas/GeoJSON.position""
        }
      }
    }
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeometryMultiPointSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryMultiPointSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""required"": [
    ""type"",
    ""coordinates""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""type"": {
      ""enum"": [
        ""MultiPoint""
      ]
    },
    ""coordinates"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/components/schemas/GeoJSON.position""
      }
    }
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeometryMultiLineStringSchemaSerializeAsYamlWorks() // Test yaml
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryMultiLineStringSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string yaml = await schema.SerializeAsYamlAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"required:
  - type
  - coordinates
type: object
properties:
  type:
    enum:
      - MultiLineString
  coordinates:
    minItems: 2
    type: array
    items:
      type: array
      items:
        $ref: '#/components/schemas/GeoJSON.position'".ChangeLineBreaks(), yaml);
        }

        [Fact]
        public async Task CreateEdmGeometryMultiPolygonSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryMultiPolygonSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""required"": [
    ""type"",
    ""coordinates""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""type"": {
      ""enum"": [
        ""MultiPolygon""
      ]
    },
    ""coordinates"": {
      ""minItems"": 4,
      ""type"": ""array"",
      ""items"": {
        ""type"": ""array"",
        ""items"": {
          ""type"": ""array"",
          ""items"": {
            ""$ref"": ""#/components/schemas/GeoJSON.position""
          }
        }
      }
    }
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateEdmGeometryCollectionSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateEdmGeometryCollectionSchema(new());
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""required"": [
    ""type"",
    ""coordinates""
  ],
  ""type"": ""object"",
  ""properties"": {
    ""type"": {
      ""enum"": [
        ""GeometryCollection""
      ]
    },
    ""coordinates"": {
      ""type"": ""array"",
      ""items"": {
        ""$ref"": ""#/components/schemas/Edm.Geometry""
      }
    }
  }
}".ChangeLineBreaks(), json);
        }

        [Fact]
        public async Task CreateGeoJSON_PositionSchemaSerializeAsJsonWorks()
        {
            // Arrange
            var schema = OpenApiSpatialTypeSchemaGenerator.CreateGeoJsonPointSchema();
            Assert.NotNull(schema); // guard

            // Act
            string json = await schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);

            // Assert
            Assert.Equal(@"{
  ""minItems"": 2,
  ""type"": ""array"",
  ""items"": {
    ""type"": ""number""
  }
}".ChangeLineBreaks(), json);
        }
    }
}
