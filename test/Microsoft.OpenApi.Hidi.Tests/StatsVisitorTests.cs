using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests;

public class StatsVisitorTests
{
    [Fact]
    public void GetStatisticsReportReflectsVisitedElements()
    {
        var document = new OpenApiDocument
        {
            Paths = new()
            {
                ["/pets"] = new OpenApiPathItem
                {
                    Operations = new()
                    {
                        [HttpMethod.Post] = new OpenApiOperation
                        {
                            Parameters =
                            [
                                new OpenApiParameter
                                {
                                    Name = "expand",
                                    In = ParameterLocation.Query,
                                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                                }
                            ],
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, IOpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = JsonSchemaType.Object,
                                            Properties = new Dictionary<string, IOpenApiSchema>
                                            {
                                                ["name"] = new OpenApiSchema { Type = JsonSchemaType.String }
                                            }
                                        }
                                    }
                                }
                            },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Headers = new Dictionary<string, IOpenApiHeader>
                                    {
                                        ["x-rate-limit"] = new OpenApiHeader
                                        {
                                            Schema = new OpenApiSchema { Type = JsonSchemaType.Integer }
                                        }
                                    },
                                    Links = new Dictionary<string, IOpenApiLink>
                                    {
                                        ["next"] = new OpenApiLink()
                                    }
                                }
                            },
                            Callbacks = new Dictionary<string, IOpenApiCallback>
                            {
                                ["onData"] = new OpenApiCallback
                                {
                                    PathItems = new Dictionary<RuntimeExpression, IOpenApiPathItem>
                                    {
                                        [RuntimeExpression.Build("$request.body#/callbackUrl")] = new OpenApiPathItem
                                        {
                                            Operations = new()
                                            {
                                                [HttpMethod.Post] = new OpenApiOperation
                                                {
                                                    Responses = new OpenApiResponses
                                                    {
                                                        ["202"] = new OpenApiResponse { Description = "Accepted" }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var visitor = new StatsVisitor();
        new OpenApiWalker(visitor).Walk(document);
        var report = visitor.GetStatisticsReport();

        Assert.Equal(2, visitor.PathItemCount);
        Assert.Equal(2, visitor.OperationCount);
        Assert.Equal(1, visitor.ParameterCount);
        Assert.Equal(1, visitor.RequestBodyCount);
        Assert.Equal(2, visitor.ResponseCount);
        Assert.Equal(1, visitor.LinkCount);
        Assert.Equal(1, visitor.CallbackCount);
        Assert.Equal(4, visitor.SchemaCount);
        visitor.Visit(new Dictionary<string, IOpenApiHeader> { ["x-rate-limit"] = new OpenApiHeader() });
        Assert.Equal(1, visitor.HeaderCount);
        var expected = string.Join(Environment.NewLine,
        [
            "Path Items: 2", "Operations: 2", "Parameters: 1", "Request Bodies: 1",
            "Responses: 2", "Links: 1", "Callbacks: 1", "Schemas: 4", ""
        ]);
        Assert.Equal(expected, report);

        visitor.Visit((IOpenApiParameter)new OpenApiParameter());

        Assert.Equal(2, visitor.ParameterCount);
        Assert.Contains("Parameters: 2", visitor.GetStatisticsReport(), StringComparison.Ordinal);
    }
}
