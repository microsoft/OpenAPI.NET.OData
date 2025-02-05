// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OpenApi.Models;
using Xunit;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Tests;

public class OpenApiOperationExtensionsTests
{
	[Theory]
	[InlineData(true, true)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(false, false)]
	public void AddsErrorResponses(bool addNoContent, bool errorAsDefault)
	{
		// Arrange
		var settings = new OpenApiConvertSettings {
			ErrorResponsesAsDefault = errorAsDefault,
		};
		var operation = new OpenApiOperation();

		// Act
		operation.AddErrorResponses(settings, addNoContent);

		// Assert
		Assert.NotNull(operation.Responses);
		Assert.Equal((errorAsDefault ? 1 : 2) + (addNoContent ? 1 : 0), operation.Responses.Count);

		if(addNoContent)
		{
			Assert.True(operation.Responses.ContainsKey("204"));
		}
		else
		{
			Assert.False(operation.Responses.ContainsKey("204"));
		}
		if(errorAsDefault)
		{
			Assert.True(operation.Responses.ContainsKey("default"));
			Assert.False(operation.Responses.ContainsKey("4XX"));
			Assert.False(operation.Responses.ContainsKey("5XX"));
		}
		else
		{
			Assert.False(operation.Responses.ContainsKey("default"));
			Assert.True(operation.Responses.ContainsKey("4XX"));
			Assert.True(operation.Responses.ContainsKey("5XX"));
		}
	}
}