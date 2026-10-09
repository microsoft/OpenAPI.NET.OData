// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.CommandLine;
using System.CommandLine.Invocation;
using Xunit;

namespace Microsoft.OpenApi.Hidi.Tests
{
    public sealed class CommandHandlerRegressionTests
    {
        [Theory]
        [InlineData("SampleOpenApi.yml", 0)]
        [InlineData("InvalidSampleOpenApi.yml", -1)]
        public async Task ValidateCommandReturnsDocumentValidationResult(string fixture, int expected)
        {
            var root = Program.CreateRootCommand();
            var command = root.Subcommands.Single(c => c.Name == "validate");
            var input = Path.Combine(AppContext.BaseDirectory, "UtilityFiles", fixture);
            var result = root.Parse(new[] { "validate", "--openapi", input });
            Assert.Empty(result.Errors);
            var action = Assert.IsType<AsynchronousCommandLineAction>(command.Action, exactMatch: false);

            Assert.Equal(expected, await action.InvokeAsync(result, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ValidateCommandReturnsCancellationWithoutReportingInvalidDocument()
        {
            var root = Program.CreateRootCommand();
            var command = root.Subcommands.Single(c => c.Name == "validate");
            var input = Path.Combine(AppContext.BaseDirectory, "UtilityFiles", "SampleOpenApi.yml");
            var result = root.Parse(new[] { "validate", "--openapi", input });
            Assert.Empty(result.Errors);
            var action = Assert.IsType<AsynchronousCommandLineAction>(command.Action, exactMatch: false);
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();

            Assert.Equal(0, await action.InvokeAsync(result, cancellation.Token));
        }

        [Theory]
        [InlineData("validate")]
        [InlineData("transform")]
        [InlineData("show")]
        [InlineData("plugin")]
        public async Task CommandReportsMissingInputFiles(string name)
        {
            var root = Program.CreateRootCommand();
            var command = root.Subcommands.Single(c => c.Name == name);
            var missing = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.yml");
            var option = name == "plugin" ? "--manifest" : "--openapi";
            var result = root.Parse(new[] { name, option, missing });
            Assert.Empty(result.Errors);
            var action = Assert.IsType<AsynchronousCommandLineAction>(command.Action, exactMatch: false);

#if DEBUG
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => action.InvokeAsync(result, TestContext.Current.CancellationToken));
            Assert.Contains("Could not", error.Message, StringComparison.Ordinal);
#else
            Assert.Equal(1, await action.InvokeAsync(result, TestContext.Current.CancellationToken));
#endif
        }

        [Fact]
        public async Task ValidateCommandRequiresAnOpenApiFile()
        {
            var root = Program.CreateRootCommand();
            var command = root.Subcommands.Single(c => c.Name == "validate");
            var result = root.Parse(["validate"]);
            var action = Assert.IsType<AsynchronousCommandLineAction>(command.Action, exactMatch: false);

#if DEBUG
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => action.InvokeAsync(result, TestContext.Current.CancellationToken));
            Assert.Equal("OpenApi file is required", error.Message);
#else
            Assert.Equal(1, await action.InvokeAsync(result, TestContext.Current.CancellationToken));
#endif
        }
    }
}
