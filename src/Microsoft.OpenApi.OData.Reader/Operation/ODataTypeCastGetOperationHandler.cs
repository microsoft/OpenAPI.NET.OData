// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;

namespace Microsoft.OpenApi.OData.Operation;

/// <summary>
/// Retrieves a .../namespace.typename get
/// </summary>
internal class ODataTypeCastGetOperationHandler : OperationHandler
{
	/// <inheritdoc/>
	public override OperationType OperationType => OperationType.Get;

	/// <summary>
	/// Gets/sets the segment before cast.
	/// this segment could be "entity set", "Collection property", etc.
	/// </summary>
	internal ODataSegment LastSecondSegment { get; set; }

	private IEdmEntityType parentEntityType;
	private IEdmEntityType targetEntityType;
	private const int SecondLastSegmentIndex = 2;
	/// <inheritdoc/>
	protected override void Initialize(ODataContext context, ODataPath path)
	{
		base.Initialize(context, path);

		// get the last second segment
		int count = path.Segments.Count;
		if(count >= SecondLastSegmentIndex)
			LastSecondSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex);

		parentEntityType = LastSecondSegment.EntityType;
		if(path.Last() is ODataTypeCastSegment oDataTypeCastSegment)
		{
			targetEntityType = oDataTypeCastSegment.EntityType;
		}
		else throw new NotImplementedException($"type cast type {path.Last().GetType().FullName} not implemented");

	}

	/// <inheritdoc/>
	protected override void SetBasicInfo(OpenApiOperation operation)
	{
		// Summary
		operation.Summary = $"Get the items of type {targetEntityType.ShortQualifiedName()} in the {parentEntityType.ShortQualifiedName()} collection";

		// OperationId
		if (Context.Settings.EnableOperationId)
		{
			operation.OperationId = $"Get.{parentEntityType.ShortQualifiedName()}.As.{targetEntityType.ShortQualifiedName()}";
		}

		base.SetBasicInfo(operation);
	}

	/// <inheritdoc/>
	protected override void SetResponses(OpenApiOperation operation)
	{

		OpenApiSchema schema = null;

		if (Context.Settings.EnableDerivedTypesReferencesForResponses)
		{
			schema = EdmModelHelper.GetDerivedTypesReferenceSchema(parentEntityType, Context.Model);
		}

		if (schema == null)
		{
			schema = new OpenApiSchema
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.Schema,
					Id = $"{parentEntityType.FullName()}.To.{targetEntityType.FullName()}"
				}
			};
		}

		var properties = new Dictionary<string, OpenApiSchema>
		{
			{
				"value",
				new OpenApiSchema
				{
					Type = "array",
					Items = schema
				}
			}
		};

		if (Context.Settings.EnablePagination)
		{
			properties.Add(
				"@odata.nextLink",
				new OpenApiSchema
				{
					Type = "string"
				});
		}

		operation.Responses = new OpenApiResponses
		{
			{
				Constants.StatusCode200,
				new OpenApiResponse
				{
					Description = "Retrieved entities",
					Content = new Dictionary<string, OpenApiMediaType>
					{
						{
							Constants.ApplicationJsonMediaType,
							new OpenApiMediaType
							{
								Schema = new OpenApiSchema
								{
									Title = $"Collection of items of type {targetEntityType.ShortQualifiedName()} in the {parentEntityType.ShortQualifiedName()} collection",
									Type = "object",
									Properties = properties
								}
							}
						}
					}
				}
			}
		};

		operation.Responses.Add(Constants.StatusCodeDefault, Constants.StatusCodeDefault.GetResponse());

		base.SetResponses(operation);
	}
	//TODO query parameters?
	//TODO extensions?
}
//TODO unit tests