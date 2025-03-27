// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

namespace Microsoft.OpenApi.OData.Operation;

internal class ComplexPropertyGetOperationHandler : ComplexPropertyBaseOperationHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComplexPropertyGetOperationHandler"/> class.
    /// </summary>
    /// <param name="document">The document to use to lookup references.</param>
    public ComplexPropertyGetOperationHandler(OpenApiDocument document):base(document)
    {
        
    }
    /// <inheritdoc />
    public override HttpMethod OperationType => HttpMethod.Get;

    private ReadRestrictionsType? _readRestrictions;

    protected override void Initialize(ODataContext context, ODataPath path)
    {
        base.Initialize(context, path);

        _readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
        var complexPropertyReadRestrictions = Context?.Model.GetRecord<ReadRestrictionsType>(ComplexPropertySegment.Property, CapabilitiesConstants.ReadRestrictions);
        _readRestrictions?.MergePropertiesIfNull(complexPropertyReadRestrictions);
        _readRestrictions ??= complexPropertyReadRestrictions;
    }

    /// <inheritdoc/>
    protected override void SetBasicInfo(OpenApiOperation operation)
    {
        // OperationId
        if (Context is {Settings.EnableOperationId: true} && Path is not null)
        {
            string prefix = ComplexPropertySegment.Property.Type.IsCollection() ? "List" : "Get";
            operation.OperationId = EdmModelHelper.GenerateComplexPropertyPathOperationId(Path, Context, prefix);
        }

        // Summary and Description
        string placeHolder = $"Get {ComplexPropertySegment.Property.Name} property value";
        operation.Summary = _readRestrictions?.Description ?? placeHolder;
        operation.Description = _readRestrictions?.LongDescription ?? Context?.Model.GetDescriptionAnnotation(ComplexPropertySegment.Property);

        base.SetBasicInfo(operation);
    }
    /// <inheritdoc/>
    protected override void SetParameters(OpenApiOperation operation)
    {
        base.SetParameters(operation);

        if (Context is null) return;

        IOpenApiParameter? parameter;
        operation.Parameters ??= [];
        if(ComplexPropertySegment.Property.Type.IsCollection())
        {
            // The parameters array contains Parameter Objects for all system query options allowed for this collection,
            // and it does not list system query options not allowed for this collection, see terms
            // Capabilities.TopSupported, Capabilities.SkipSupported, Capabilities.SearchRestrictions,
            // Capabilities.FilterRestrictions, and Capabilities.CountRestrictions
            // $top
            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateTop(TargetPath, _document)) ?? Context.CreateTop(ComplexPropertySegment.Property, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $skip
            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSkip(TargetPath, _document)) ?? Context.CreateSkip(ComplexPropertySegment.Property, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $search
            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSearch(TargetPath, _document)) ?? Context.CreateSearch(ComplexPropertySegment.Property, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $filter
            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateFilter(TargetPath, _document)) ?? Context.CreateFilter(ComplexPropertySegment.Property, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // $count
            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateCount(TargetPath, _document)) ?? Context.CreateCount(ComplexPropertySegment.Property, _document);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }

            // the syntax of the system query options $expand, $select, and $orderby is too flexible
            // to be formally described with OpenAPI Specification means, yet the typical use cases
            // of just providing a comma-separated list of properties can be expressed via an array-valued
            // parameter with an enum constraint
            // $order
            parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateOrderBy(TargetPath, ComplexPropertySegment.ComplexType)) 
                ?? Context.CreateOrderBy(ComplexPropertySegment.Property, ComplexPropertySegment.ComplexType);
            if (parameter != null)
            {
                operation.Parameters.Add(parameter);
            }
        }

        // $select
        parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSelect(TargetPath, ComplexPropertySegment.ComplexType)) 
            ?? Context.CreateSelect(ComplexPropertySegment.Property, ComplexPropertySegment.ComplexType);
        if (parameter != null)
        {
            operation.Parameters.Add(parameter);
        }

        // $expand
        parameter = (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateExpand(TargetPath, ComplexPropertySegment.ComplexType)) 
            ?? Context.CreateExpand(ComplexPropertySegment.Property, ComplexPropertySegment.ComplexType);
        if (parameter != null)
        {
            operation.Parameters.Add(parameter);
        }
    }

    protected override void SetExtensions(OpenApiOperation operation)
    {
        if (Context is {Settings.EnablePagination: true} && ComplexPropertySegment.Property.Type.IsCollection())
        {
            JsonObject extension = new()
			{
                { "nextLinkName", "@odata.nextLink"},
                { "operationName", Context.Settings.PageableOperationName}
            };
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            operation.Extensions.Add(Constants.xMsPageable, new OpenApiAny(extension));

            base.SetExtensions(operation);
        }
    }
    /// <inheritdoc/>
    protected override void SetResponses(OpenApiOperation operation)
    {
        if (ComplexPropertySegment.Property.Type.IsCollection())
        {
            SetCollectionResponse(operation, ComplexPropertySegment.ComplexType.FullName());
        }
        else
        {
            var schema = new OpenApiSchemaReference(ComplexPropertySegment.ComplexType.FullName(), _document);

            SetSingleResponse(operation, schema);
        }

        if (Context is not null)
            operation.AddErrorResponses(Context.Settings, _document, false);
        base.SetResponses(operation);
    }
   
    protected override void SetSecurity(OpenApiOperation operation)
    {
        if (_readRestrictions?.Permissions == null)
        {
            return;
        }

        operation.Security = Context?.CreateSecurityRequirements(_readRestrictions.Permissions, _document).ToList();
    }

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {        
        if (_readRestrictions == null)
        {
            return;
        }

        if (_readRestrictions.CustomHeaders != null)
        {
            AppendCustomParameters(operation, _readRestrictions.CustomHeaders, ParameterLocation.Header);
        }

        if (_readRestrictions.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, _readRestrictions.CustomQueryOptions, ParameterLocation.Query);
        }
    }
}