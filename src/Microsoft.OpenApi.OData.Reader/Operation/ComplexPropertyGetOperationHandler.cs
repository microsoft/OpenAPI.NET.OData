// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyGetOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <inheritdoc />
    public override OperationType OperationType => OperationType.Get;
    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // Summary
        operation.Summary = $"Get {ComplexPropertySegment.Property.Name} property value";

        // OperationId
        if (Context.Settings.EnableOperationId)
        {
            string typeName = ComplexPropertySegment.ComplexType.Name;
            string listOrGet = ComplexPropertySegment.Property.Type.IsCollection() ? ".List" : ".Get";
            operation.OperationId = ComplexPropertySegment.Property.Name + "." + typeName + listOrGet + Utils.UpperFirstChar(typeName);
        }

        base.SetBasicInfo(operation);
    }
    /// <inheritdoc/>
    protected override void SetParameters(OpenApiOperation operation)
    {
        base.SetParameters(operation);

        OpenApiParameter parameter;
        if(ComplexPropertySegment.Property.Type.IsCollection())
        {

            // The parameters array contains Parameter Objects for all system query options allowed for this collection,
            // and it does not list system query options not allowed for this collection, see terms
            // Capabilities.TopSupported, Capabilities.SkipSupported, Capabilities.SearchRestrictions,
            // Capabilities.FilterRestrictions, and Capabilities.CountRestrictions
            // $top
            parameter = Context.CreateTop(ComplexPropertySegment.Property);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $skip
            parameter = Context.CreateSkip(ComplexPropertySegment.Property);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $search
            parameter = Context.CreateSearch(ComplexPropertySegment.Property);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $filter
            parameter = Context.CreateFilter(ComplexPropertySegment.Property);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $count
            parameter = Context.CreateCount(ComplexPropertySegment.Property);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // the syntax of the system query options $expand, $select, and $orderby is too flexible
            // to be formally described with OpenAPI Specification means, yet the typical use cases
            // of just providing a comma-separated list of properties can be expressed via an array-valued
            // parameter with an enum constraint
            // $order
            parameter = Context.CreateOrderBy(ComplexPropertySegment.Property, ComplexPropertySegment.ComplexType);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        // $select
        parameter = Context.CreateSelect(ComplexPropertySegment.Property, ComplexPropertySegment.ComplexType);
        if (parameter != null)
        {
            operation.Parameters.Add(parameter);
        }

        // $expand
        parameter = Context.CreateExpand(ComplexPropertySegment.Property, ComplexPropertySegment.ComplexType);
        if (parameter != null)
        {
            operation.Parameters.Add(parameter);
        }
    }

    protected override void SetExtensions(OpenApiOperation operation)
    {
        if (Context.Settings.EnablePagination && ComplexPropertySegment.Property.Type.IsCollection())
        {
            OpenApiObject extension = new()
			{
                { "nextLinkName", new OpenApiString("@odata.nextLink")},
                { "operationName", new OpenApiString(Context.Settings.PageableOperationName)}
            };
            operation.Extensions.Add(Constants.xMsPageable, extension);

            base.SetExtensions(operation);
        }
    }
    /// <inheritdoc/>
    protected override void SetResponses(OpenApiOperation operation)
    {
        if(ComplexPropertySegment.Property.Type.IsCollection())
            SetCollectionResponse(operation);
        else
            SetSingleResponse(operation);
        operation.AddErrorResponses(Context.Settings, false);

        base.SetResponses(operation);
    }
    private void SetCollectionResponse(OpenApiOperation operation)
    {
        operation.Responses = new OpenApiResponses
        {
            {
                Constants.StatusCode200,
                new OpenApiResponse
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.Response,
                        Id = $"{ComplexPropertySegment.ComplexType.FullName()}{Constants.CollectionSchemaSuffix}"
                    },
                }
            }
        };
    }
    private void SetSingleResponse(OpenApiOperation operation)
    {
        OpenApiSchema schema = null;

        if (schema == null)
        {
            schema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = ComplexPropertySegment.ComplexType.FullName()
                }
            };
        }
        operation.Responses = new OpenApiResponses
        {
            {
                Constants.StatusCode200,
                new OpenApiResponse
                {
                    Description = "Result entities",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            Constants.ApplicationJsonMediaType,
                            new OpenApiMediaType
                            {
                                Schema = schema
                            }
                        }
                    },
                }
            }
        };
    }
    protected override void SetSecurity(OpenApiOperation operation)
    {
        ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.ReadRestrictions);
        if (read == null || read.Permissions == null)
        {
            return;
        }

        operation.Security = Context.CreateSecurityRequirements(read.Permissions).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
        ReadRestrictionsType read = Context.Model.GetRecord<ReadRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.ReadRestrictions);
        if (read == null)
        {
            return;
        }

        if (read.CustomHeaders != null)
        {
            AppendCustomParameters(operation, read.CustomHeaders, ParameterLocation.Header);
        }

        if (read.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, read.CustomQueryOptions, ParameterLocation.Query);
        }
    }
}