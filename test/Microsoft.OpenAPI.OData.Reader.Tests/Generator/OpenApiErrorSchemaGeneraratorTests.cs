// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Xunit;

namespace Microsoft.OpenApi.OData.Tests;
public class OpenApiErrorSchemaGeneraratorTests
{
    [Fact]
    public void AddsEmptyInnerErrorWhenNoComplexTypeIsProvided()
    {
        IEdmModel model = EdmModelHelper.ContractServiceModel;
        OpenApiConvertSettings settings = new()
        {
			ErrorResponsesAsDefault = false,
        };
        ODataContext context = new(model, settings);

        var schema = OpenApiErrorSchemaGenerator.CreateInnerErrorSchema(context);

        Assert.Equal("object", schema.Type);
        Assert.Empty(schema.Properties);
    }
    [Fact]
    public void AddsInnerErrorPropertiesWhenComplexTypeIsProvided()
    {
        IEdmModel model = EdmModelHelper.TripServiceModel;
        OpenApiConvertSettings settings = new()
        {
            ErrorResponsesAsDefault = false,
        };
        ODataContext context = new(model, settings);

        var schema = OpenApiErrorSchemaGenerator.CreateInnerErrorSchema(context);

        Assert.Equal("object", schema.Type);
        Assert.NotEmpty(schema.Properties);
        Assert.Contains("Date", schema.Properties.Keys);
        Assert.Contains("RequestId", schema.Properties.Keys);
    }
}