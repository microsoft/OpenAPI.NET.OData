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
	private string entityTypeName;
	private IEdmNavigationProperty navigationProperty;
	private IEdmStructuredType parentStructuredType;
	private IEdmSchemaElement ParentSchemaElement => parentStructuredType as IEdmSchemaElement;
	private IEdmStructuredType targetStructuredType;
	private IEdmSchemaElement TargetSchemaElement => targetStructuredType as IEdmSchemaElement;
	private const int SecondLastSegmentIndex = 2;
	private bool isIndexedCollValuedNavProp = false;
	private IEdmNavigationSource navigationSource;
	private IEdmVocabularyAnnotatable annotatable;

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
		isIndexedCollValuedNavProp = false;
		annotatable = null;
		base.Initialize(context, path);

		// get the last second segment
		int count = path.Segments.Count;
		if(count >= SecondLastSegmentIndex)
			LastSecondSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex);

		parentStructuredType = LastSecondSegment is ODataComplexPropertySegment complexSegment ? complexSegment.ComplexType : LastSecondSegment.EntityType;
        ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
        navigationSource = navigationSourceSegment.NavigationSource;
        entitySet = navigationSource as IEdmEntitySet;
        singleton = navigationSource as IEdmSingleton;

        if (LastSecondSegment is ODataNavigationPropertySegment navigationPropertySegment)
		{
			SetNavigationPropertyAndRestrictionFromNavigationSegment(navigationPropertySegment, path);
		}
		else if(LastSecondSegment is ODataNavigationSourceSegment sourceSegment)
		{
			SetAnnotatableRestrictionFromNavigationSourceSegment(sourceSegment);
        }
		else if(LastSecondSegment is ODataKeySegment)
		{
			isKeySegment = true;
			var thirdLastSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex - 1);
			if (thirdLastSegment is ODataNavigationPropertySegment navigationPropertySegment1)
			{
                isIndexedCollValuedNavProp = true;
                SetNavigationPropertyAndRestrictionFromNavigationSegment(navigationPropertySegment1, path);				
			}
			else if (thirdLastSegment is ODataNavigationSourceSegment sourceSegment1)
			{
				SetAnnotatableRestrictionFromNavigationSourceSegment(sourceSegment1);
            }
		}

		if (path.Last() is ODataTypeCastSegment odataTypeCastSegment)
		{
			targetStructuredType = odataTypeCastSegment.StructuredType;
		}
		else 
		{
            throw new NotImplementedException($"type cast type {path.Last().GetType().FullName} not implemented");
        }
    }

	private void SetNavigationPropertyAndRestrictionFromNavigationSegment(ODataNavigationPropertySegment navigationPropertySegment, ODataPath path)
	{
		navigationProperty = navigationPropertySegment.NavigationProperty;
		annotatable = navigationProperty;
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

    private void SetAnnotatableRestrictionFromNavigationSourceSegment(ODataNavigationSourceSegment sourceSegment)
	{
        if (sourceSegment.NavigationSource is IEdmEntitySet eSet)
        {
			annotatable = eSet;   

        }
		else if (sourceSegment.NavigationSource is IEdmSingleton sTon)
		{
			annotatable = sTon;
		}

        SetRestrictionFromAnnotatable(annotatable);
    }
	    

	private void SetRestrictionFromAnnotatable(IEdmVocabularyAnnotatable annotatable)
	{
		if (this.annotatable == null)
			return;

		NavigationRestrictionsType navigation = Context.Model.GetRecord<NavigationRestrictionsType>(annotatable, CapabilitiesConstants.NavigationRestrictions);
		if (navigation?.RestrictedProperties != null)
		{
			restriction = navigation.RestrictedProperties.FirstOrDefault(r => r.NavigationProperty == null);
		}
	}

	/// <inheritdoc/>
	protected override void SetBasicInfo(OpenApiOperation operation)
	{
		// Summary
		if (IsSingleElement)
			operation.Summary = $"Get the item of type {ParentSchemaElement.ShortQualifiedName()} as {TargetSchemaElement.ShortQualifiedName()}";
		else
			operation.Summary = $"Get the items of type {TargetSchemaElement.ShortQualifiedName()} in the {ParentSchemaElement.ShortQualifiedName()} collection";

		// OperationId
		if (Context.Settings.EnableOperationId)
            operation.OperationId = CreateOperationId(Path);

        base.SetBasicInfo(operation);
	}

	/// <inheritdoc/>
	protected override void SetResponses(OpenApiOperation operation)
	{
		if (IsSingleElement)
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
				Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
				new OpenApiResponse
				{
					UnresolvedReference = true,
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
				UnresolvedReference = true,
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
				Context.Settings.UseSuccessStatusCodeRange ? Constants.StatusCodeClass2XX : Constants.StatusCode200,
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
		string tagName = null;
		
        if (LastSecondSegment is ODataNavigationPropertySegment || isIndexedCollValuedNavProp)
		{
            IEdmNavigationSource navigationSource = (entitySet != null) ? entitySet : singleton;
            tagName = EdmModelHelper.GenerateNavigationPropertyPathTag(Path, navigationSource, navigationProperty, Context);
        }
        else if ((LastSecondSegment is ODataKeySegment && !isIndexedCollValuedNavProp)
				|| (LastSecondSegment is ODataNavigationSourceSegment))
        {
            tagName = entitySet != null
				? entitySet.Name + "." + entitySet.EntityType().Name
				: singleton.Name + "." + singleton.EntityType().Name;
        }

        OpenApiTag tag = new()
		{
			Name = tagName
        };

		if (!IsSingleElement)
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

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
		if (annotatable == null)
			return;
        
        ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(annotatable, CapabilitiesConstants.ReadRestrictions);

        if (readRestrictions == null)
        {
            return;
        }

        if (readRestrictions.CustomHeaders != null)
        {
            AppendCustomParameters(operation, readRestrictions.CustomHeaders, ParameterLocation.Header);
        }

        if (readRestrictions.CustomQueryOptions != null)
        {
            AppendCustomParameters(operation, readRestrictions.CustomQueryOptions, ParameterLocation.Query);
        }
    }

	private string CreateOperationId(ODataPath path)
	{
		string operationId = null;

		if (LastSecondSegment is ODataComplexPropertySegment)
		{
            ODataComplexPropertySegment complexPropertySegment = parentStructuredType as ODataComplexPropertySegment;
            string typeName = complexPropertySegment.ComplexType.Name;
            string listOrGet = complexPropertySegment.Property.Type.IsCollection() ? ".List" : ".Get";
            operationId = complexPropertySegment.Property.Name + "." + typeName + listOrGet + Utils.UpperFirstChar(typeName);
        }
		else if (LastSecondSegment is ODataNavigationPropertySegment || isIndexedCollValuedNavProp)
		{
            string prefix = "Get";
            if (!isIndexedCollValuedNavProp && navigationProperty.TargetMultiplicity() == EdmMultiplicity.Many)
			{
                prefix = "List";
            }

			IEdmNavigationSource navigationSource = (entitySet != null) ? entitySet : singleton;
			operationId = EdmModelHelper.GenerateNavigationPropertyPathOperationId(path, navigationSource, prefix);			
		}
		else if (LastSecondSegment is ODataKeySegment keySegment && !isIndexedCollValuedNavProp)
		{
            string operationName = $"Get{Utils.UpperFirstChar(entityTypeName)}";
            if (keySegment.IsAlternateKey)
            {
                string alternateKeyName = string.Join("", keySegment.Identifier.Split(',').Select(static x => Utils.UpperFirstChar(x)));
                operationName = $"{operationName}By{alternateKeyName}";
            }
            operationId = $"{entitySet.Name}.{entityTypeName}.{operationName}";
        }
		else if (LastSecondSegment is ODataNavigationSourceSegment)
		{
            operationId = (entitySet != null)
				? entitySet.Name + "." + entityTypeName + ".List" + Utils.UpperFirstChar(entityTypeName)
				: singleton.Name + "." + entityTypeName + ".Get" + Utils.UpperFirstChar(entityTypeName);
        }

		if (operationId != null)
		{
			operationId = $"{operationId}.As{Utils.UpperFirstChar(TargetSchemaElement.Name)}";
        }

		return operationId;
    }
}