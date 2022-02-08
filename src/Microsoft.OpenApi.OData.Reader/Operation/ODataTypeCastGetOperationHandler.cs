// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Generator;
using Microsoft.OpenApi.OData.Vocabulary.Capabilities;

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

	private bool isKeySegment;
	private bool IsSingleElement 
	{
		get => isKeySegment ||
				singleton != null ||
					(navigationProperty != null &&
					!navigationProperty.Type.IsCollection() &&
					entitySet == null);
	}
	private NavigationPropertyRestriction restriction;
	private IEdmSingleton singleton;
	private IEdmEntitySet entitySet;
	private IEdmNavigationProperty navigationProperty;
	private IEdmStructuredType parentStructuredType;
	private IEdmSchemaElement ParentSchemaElement => parentStructuredType as IEdmSchemaElement;
	private IEdmStructuredType targetStructuredType;
	private IEdmSchemaElement TargetSchemaElement => targetStructuredType as IEdmSchemaElement;
	private const int SecondLastSegmentIndex = 2;
	/// <inheritdoc/>
	protected override void Initialize(ODataContext context, ODataPath path)
	{
		// reseting the fields as we're reusing the handler
		singleton = null;
		isKeySegment = false;
		restriction = null;
		entitySet = null;
		navigationProperty = null;
		parentStructuredType = null;
		targetStructuredType = null;
		base.Initialize(context, path);

		// get the last second segment
		int count = path.Segments.Count;
		if(count >= SecondLastSegmentIndex)
			LastSecondSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex);

		parentStructuredType = LastSecondSegment is ODataComplexPropertySegment complexSegment ? complexSegment.ComplexType : LastSecondSegment.EntityType;
		if(LastSecondSegment is ODataNavigationPropertySegment navigationPropertySegment)
		{
			SetNavigationPropertyAndRestrictionFromNavigationSegment(navigationPropertySegment, path);
		}
		else if(LastSecondSegment is ODataNavigationSourceSegment sourceSegment)
		{
			if(sourceSegment.NavigationSource is IEdmEntitySet)
				SetEntitySetAndRestrictionFromSourceSegment(sourceSegment);
			else if (sourceSegment.NavigationSource is IEdmSingleton)
				SetSingletonAndRestrictionFromSourceSegment(sourceSegment);
		}
		else if(LastSecondSegment is ODataKeySegment)
		{
			isKeySegment = true;
			var thirdLastSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex - 1);
			if(thirdLastSegment is ODataNavigationPropertySegment navigationPropertySegment1)
			{
				SetNavigationPropertyAndRestrictionFromNavigationSegment(navigationPropertySegment1, path);
			}
			else if(thirdLastSegment is ODataNavigationSourceSegment sourceSegment1)
			{
				SetEntitySetAndRestrictionFromSourceSegment(sourceSegment1);
			}
		}
		if(path.Last() is ODataTypeCastSegment oDataTypeCastSegment)
		{
			targetStructuredType = oDataTypeCastSegment.StructuredType;
		}
		else throw new NotImplementedException($"type cast type {path.Last().GetType().FullName} not implemented");
	}

	private void SetNavigationPropertyAndRestrictionFromNavigationSegment(ODataNavigationPropertySegment navigationPropertySegment, ODataPath path)
	{
		navigationProperty = navigationPropertySegment.NavigationProperty;
		var navigationPropertyPath = string.Join("/",
			Path.Segments.Where(s => !(s is ODataKeySegment || s is ODataNavigationSourceSegment
									|| s is ODataStreamContentSegment || s is ODataStreamPropertySegment)).Select(e => e.Identifier));

		if(path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment)
		{
			NavigationRestrictionsType navigation = navigationSourceSegment.NavigationSource switch {
				IEdmEntitySet eSet => Context.Model.GetRecord<NavigationRestrictionsType>(eSet, CapabilitiesConstants.NavigationRestrictions),
				IEdmSingleton single => Context.Model.GetRecord<NavigationRestrictionsType>(single, CapabilitiesConstants.NavigationRestrictions),
				_ => null
			};

			if (navigation?.RestrictedProperties != null)
			{
				restriction = navigation.RestrictedProperties.FirstOrDefault(r => r.NavigationProperty != null && r.NavigationProperty == navigationPropertyPath);
			}
		}
	}

	private void SetEntitySetAndRestrictionFromSourceSegment(ODataNavigationSourceSegment sourceSegment)
	{
		if(sourceSegment.NavigationSource is IEdmEntitySet eSet)
		{
			entitySet = eSet;
			SetRestrictionFromAnnotable(eSet);
		}
	}
	
	private void SetSingletonAndRestrictionFromSourceSegment(ODataNavigationSourceSegment sourceSegment)
	{
		if(sourceSegment.NavigationSource is IEdmSingleton sTon)
		{
			singleton = sTon;
			SetRestrictionFromAnnotable(sTon);
		}

	}

	private void SetRestrictionFromAnnotable(IEdmVocabularyAnnotatable annotable)
	{
		NavigationRestrictionsType navigation = Context.Model.GetRecord<NavigationRestrictionsType>(annotable, CapabilitiesConstants.NavigationRestrictions);
		if (navigation?.RestrictedProperties != null)
		{
			restriction = navigation.RestrictedProperties.FirstOrDefault(r => r.NavigationProperty == null);
		}
	}

	/// <inheritdoc/>
	protected override void SetBasicInfo(OpenApiOperation operation)
	{
		// Summary
		if(IsSingleElement)
			operation.Summary = $"Get the item of type {ParentSchemaElement.ShortQualifiedName()} as {TargetSchemaElement.ShortQualifiedName()}";
		else
			operation.Summary = $"Get the items of type {TargetSchemaElement.ShortQualifiedName()} in the {ParentSchemaElement.ShortQualifiedName()} collection";

		// OperationId
		if (Context.Settings.EnableOperationId)
		{
			var operationItem = IsSingleElement ? ".Item" : ".Items";
			operation.OperationId = $"Get.{ParentSchemaElement.ShortQualifiedName()}{operationItem}.As.{TargetSchemaElement.ShortQualifiedName()}-{Path.GetPathHash(Context.Settings)}";
		}

		base.SetBasicInfo(operation);
	}

	/// <inheritdoc/>
	protected override void SetResponses(OpenApiOperation operation)
	{
		if(IsSingleElement)
			SetSingleResponse(operation);
		else
			SetCollectionResponse(operation);

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
						Id = $"{TargetSchemaElement.FullName()}{Constants.CollectionSchemaSuffix}"
					},
				}
			}
		};
	}
	private void SetSingleResponse(OpenApiOperation operation)
	{
		OpenApiSchema schema = null;

		if (Context.Settings.EnableDerivedTypesReferencesForResponses)
		{
			schema = EdmModelHelper.GetDerivedTypesReferenceSchema(targetStructuredType, Context.Model);
		}

		if (schema == null)
		{
			schema = new OpenApiSchema
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.Schema,
					Id = TargetSchemaElement.FullName()
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
	/// <inheritdoc/>
	protected override void SetTags(OpenApiOperation operation)
	{
		IList<string> items = new List<string>
		{
			ParentSchemaElement.Name,
			TargetSchemaElement.Name,
		};

		string name = string.Join(".", items);
		OpenApiTag tag = new()
		{
			Name = name
		};
		if(!IsSingleElement)
			tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));
		operation.Tags.Add(tag);

		Context.AppendTag(tag);

		base.SetTags(operation);
	}
	/// <inheritdoc/>
	protected override void SetParameters(OpenApiOperation operation)
	{
		base.SetParameters(operation);

		if(navigationProperty != null) {
			if (IsSingleElement)
			{
				new OpenApiParameter[] {
						Context.CreateSelect(navigationProperty),
						Context.CreateExpand(navigationProperty),
					}
				.Where(x => x != null)
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
			else
			{
				GetParametersForAnnotableOfMany(navigationProperty)
				.Union(
					new OpenApiParameter[] {
						Context.CreateOrderBy(navigationProperty),
						Context.CreateSelect(navigationProperty),
						Context.CreateExpand(navigationProperty),
					})
				.Where(x => x != null)
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
		}
		else if(entitySet != null)
		{
			if(IsSingleElement)
			{
				new OpenApiParameter[] {
						Context.CreateSelect(entitySet),
						Context.CreateExpand(entitySet),
					}
				.Where(x => x != null)
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
			else
			{
				GetParametersForAnnotableOfMany(entitySet)
				.Union(
					new OpenApiParameter[] {
						Context.CreateOrderBy(entitySet),
						Context.CreateSelect(entitySet),
						Context.CreateExpand(entitySet),
					})
				.Where(x => x != null)
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
		}
		else if(singleton != null)
		{
			new OpenApiParameter[] {
					Context.CreateSelect(singleton),
					Context.CreateExpand(singleton),
				}
			.Where(x => x != null)
			.ToList()
			.ForEach(p => operation.Parameters.Add(p));
		}
	}
	private IEnumerable<OpenApiParameter> GetParametersForAnnotableOfMany(IEdmVocabularyAnnotatable annotable) 
	{
		// Need to verify that TopSupported or others should be applied to navigation source.
		// So, how about for the navigation property.
		return new OpenApiParameter[] {
			Context.CreateTop(annotable),
			Context.CreateSkip(annotable),
			Context.CreateSearch(annotable),
			Context.CreateFilter(annotable),
			Context.CreateCount(annotable),
		};
	}

	protected override void SetSecurity(OpenApiOperation operation)
	{
		if (restriction == null || restriction.ReadRestrictions == null)
		{
			return;
		}

		ReadRestrictionsBase readBase = restriction.ReadRestrictions;

		operation.Security = Context.CreateSecurityRequirements(readBase.Permissions).ToList();
	}

	protected override void SetExtensions(OpenApiOperation operation)
	{
		if (Context.Settings.EnablePagination && !IsSingleElement)
		{
			OpenApiObject extension = new()
			{
				{ "nextLinkName", new OpenApiString("@odata.nextLink")},
				{ "operationName", new OpenApiString(Context.Settings.PageableOperationName)}
			};

			operation.Extensions.Add(Constants.xMsPageable, extension);
		}

		base.SetExtensions(operation);
	}
}