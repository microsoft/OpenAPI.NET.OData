// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiParameter"/> by Edm model.
    /// </summary>
    internal static class OpenApiParameterGenerator
    {
        /// <summary>
        /// 4.6.2 Field parameters in components
        /// Create a map of <see cref="OpenApiParameter"/> object.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <returns>The created map of <see cref="OpenApiParameter"/> object.</returns>
        public static IDictionary<string, OpenApiParameter> CreateParameters(this ODataContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // It allows defining query options and headers that can be reused across operations of the service.
            // The value of parameters is a map of Parameter Objects.
            return new Dictionary<string, OpenApiParameter>
            {
                { "top", CreateTop() },
                { "skip", CreateSkip() },
                { "count", CreateCount() },
                { "filter", CreateFilter() },
                { "search", CreateSearch() },
            };
        }

        // #top
        private static OpenApiParameter CreateTop()
        {
            return new OpenApiParameter
            {
                Name = "$top",
                In = ParameterLocation.Query,
                Description = "Show only the first n items",
                Schema = new OpenApiSchema
                {
                    Type = "integer",
                    Minimum = 0,
                },
                Example = new OpenApiInteger(50)
            };
        }

        // $skip
        private static OpenApiParameter CreateSkip()
        {
            return new OpenApiParameter
            {
                Name = "$skip",
                In = ParameterLocation.Query,
                Description = "Skip the first n items",
                Schema = new OpenApiSchema
                {
                    Type = "integer",
                    Minimum = 0,
                }
            };
        }

        // $count
        private static OpenApiParameter CreateCount()
        {
            return new OpenApiParameter
            {
                Name = "$count",
                In = ParameterLocation.Query,
                Description = "Include count of items",
                Schema = new OpenApiSchema
                {
                    Type = "boolean"
                }
            };
        }

        // $filter
        private static OpenApiParameter CreateFilter()
        {
            return new OpenApiParameter
            {
                Name = "$filter",
                In = ParameterLocation.Query,
                Description = "Filter items by property values",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            };
        }

        // $search
        private static OpenApiParameter CreateSearch()
        {
            return new OpenApiParameter
            {
                Name = "$search",
                In = ParameterLocation.Query,
                Description = "Search items by search phrases",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            };
        }
    }
}
