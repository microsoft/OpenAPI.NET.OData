// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Hidi.Options;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests
{
    public sealed class TransformFilterTests : IDisposable
    {
        private readonly string _directory = Path.Join(Path.GetTempPath(), $"hidi-transform-filters-{Guid.NewGuid():N}");
        private readonly string _inputPath;
        private readonly string _outputPath;
        private readonly string _collectionPath;

        public TransformFilterTests()
        {
            Directory.CreateDirectory(_directory);
            _inputPath = Path.Join(_directory, "input.yaml");
            _outputPath = Path.Join(_directory, "output.json");
            _collectionPath = Path.Join(_directory, "collection.json");
            File.WriteAllText(_inputPath, """
                openapi: 3.0.0
                info:
                  title: Filter test
                  version: 1.0.0
                servers:
                  - url: https://example.org
                tags:
                  - name: users
                  - name: groups
                paths:
                  /users:
                    get:
                      operationId: listUsers
                      tags: [users]
                      responses:
                        '200':
                          description: OK
                    post:
                      operationId: createUser
                      tags: [users]
                      responses:
                        '201':
                          description: Created
                  /groups:
                    get:
                      operationId: listGroups
                      tags: [groups]
                      responses:
                        '200':
                          description: OK
                """);
        }

        [Theory]
        [InlineData("listUsers", null, false)]
        [InlineData(null, "users", true)]
        public async Task TransformFiltersPathsAndOperationsAsync(string? operationIds, string? tags, bool includePost)
        {
            var options = CreateOptions();
            options.FilterOptions.FilterByOperationIds = operationIds;
            options.FilterOptions.FilterByTags = tags;

            await OpenApiService.TransformOpenApiDocumentAsync(options, NullLogger.Instance, TestContext.Current.CancellationToken);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(_outputPath, TestContext.Current.CancellationToken));
            var path = Assert.Single(document.RootElement.GetProperty("paths").EnumerateObject());
            Assert.Equal("/users", path.Name);
            Assert.Equal("listUsers", path.Value.GetProperty("get").GetProperty("operationId").GetString());
            Assert.Equal(includePost, path.Value.TryGetProperty("post", out _));
            Assert.Equal(includePost ? 2 : 1, path.Value.EnumerateObject().Count());
            if (includePost)
            {
                Assert.Equal("createUser", path.Value.GetProperty("post").GetProperty("operationId").GetString());
            }
        }

        [Fact]
        public async Task RejectCombiningOperationIdsAndTagsAsync()
        {
            var options = CreateOptions();
            options.FilterOptions.FilterByOperationIds = "listUsers";
            options.FilterOptions.FilterByTags = "users";

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                OpenApiService.TransformOpenApiDocumentAsync(options, NullLogger.Instance, TestContext.Current.CancellationToken));

            Assert.Contains("Cannot filter by operationIds and tags at the same time.", exception.Message, StringComparison.Ordinal);
            Assert.False(File.Exists(_outputPath));
        }

        [Fact]
        public async Task TransformSelectsOnlyPostmanCollectionOperationsAsync()
        {
            await File.WriteAllTextAsync(_collectionPath, """
                {
                  "item": [
                    {
                      "request": {
                        "method": "GET",
                        "url": { "raw": "https://example.org/groups" }
                      }
                    }
                  ]
                }
                """, TestContext.Current.CancellationToken);
            var options = CreateOptions();
            options.FilterOptions.FilterByCollection = _collectionPath;

            await OpenApiService.TransformOpenApiDocumentAsync(options, NullLogger.Instance, TestContext.Current.CancellationToken);

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(_outputPath, TestContext.Current.CancellationToken));
            var path = Assert.Single(document.RootElement.GetProperty("paths").EnumerateObject());
            Assert.Equal("/groups", path.Name);
            var operation = Assert.Single(path.Value.EnumerateObject());
            Assert.Equal("get", operation.Name);
            Assert.Equal("listGroups", operation.Value.GetProperty("operationId").GetString());
        }

        private HidiOptions CreateOptions()
        {
            return new HidiOptions
            {
                OpenApi = _inputPath,
                Output = new FileInfo(_outputPath),
                OpenApiFormat = OpenApiConstants.Json,
                Version = "3.0"
            };
        }

        public void Dispose()
        {
            File.Delete(_outputPath);
            File.Delete(_collectionPath);
            File.Delete(_inputPath);
            Directory.Delete(_directory);
        }
    }
}
