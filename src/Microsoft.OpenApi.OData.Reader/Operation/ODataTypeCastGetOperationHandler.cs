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
using Microsoft.OpenApi.OData.Vocabulary.Core;

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
	internal ODataSegment SecondLastSegment { get; set; }

    private bool isKeySegment;

	private bool secondLastSegmentIsComplexProperty;

	private bool IsSingleElement 
	{
		get => isKeySegment ||
				secondLastSegmentIsComplexProperty ||
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
	private bool isIndexedCollValuedNavProp = false;
	private IEdmNavigationSource navigationSource;
	private IEdmVocabularyAnnotatable annotatable;

    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
	{
		// reseting the fields as we're reusing the handler
		singleton = null;
		isKeySegment = false;
		secondLastSegmentIsComplexProperty = false;
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
			SecondLastSegment = path.Segments.ElementAt(count - SecondLastSegmentIndex);

		parentStructuredType = SecondLastSegment is ODataComplexPropertySegment complexSegment ? complexSegment.ComplexType : SecondLastSegment.EntityType;
        ODataNavigationSourceSegment navigationSourceSegment = path.FirstSegment as ODataNavigationSourceSegment;
        navigationSource = navigationSourceSegment.NavigationSource;

		if (SecondLastSegment is ODataNavigationPropertySegment navigationPropertySegment)
		{
			SetNavigationPropertyAndRestrictionFromNavigationSegment(navigationPropertySegment, path);
		}
		else if (SecondLastSegment is ODataNavigationSourceSegment sourceSegment)
		{
			SetAnnotatableRestrictionFromNavigationSourceSegment(sourceSegment);
        }
		else if (SecondLastSegment is ODataKeySegment)
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
		else if (SecondLastSegment is ODataComplexPropertySegment)
		{
			secondLastSegmentIsComplexProperty = true;
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
			entitySet = eSet;

        }
		else if (sourceSegment.NavigationSource is IEdmSingleton sTon)
		{
			annotatable = sTon;
			singleton = sTon;
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
        ReadRestrictionsType _readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);

        // Summary
        string placeHolder = IsSingleElement 
			? $"Get the item of type {ParentSchemaElement.ShortQualifiedName()} as {TargetSchemaElement.ShortQualifiedName()}" 
			: $"Get the items of type {TargetSchemaElement.ShortQualifiedName()} in the {ParentSchemaElement.ShortQualifiedName()} collection";
        operation.Summary = _readRestrictions?.Description ?? placeHolder;
        operation.Description = _readRestrictions?.LongDescription;

        // OperationId
        if (Context.Settings.EnableOperationId)
			operation.OperationId = EdmModelHelper.GenerateODataTypeCastPathOperationIdPrefix(Path, Context) + $".As{Utils.UpperFirstChar(TargetSchemaElement.Name)}";

        base.SetBasicInfo(operation);
	}

	/// <inheritdoc/>
	protected override void SetResponses(OpenApiOperation operation)
    {
        if (IsSingleElement)
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

            SetSingleResponse(operation, schema);
        }
		else
		{
            SetCollectionResponse(operation, TargetSchemaElement.FullName());
        }			

		operation.AddErrorResponses(Context.Settings, false);

		base.SetResponses(operation);
	}

	/// <inheritdoc/>
	protected override void SetTags(OpenApiOperation operation)
	{
        string tagName = null;

        if (SecondLastSegment is ODataNavigationPropertySegment || isIndexedCollValuedNavProp)
		{
			tagName = EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context);
		}
		else if ((SecondLastSegment is ODataKeySegment && !isIndexedCollValuedNavProp)
				|| (SecondLastSegment is ODataNavigationSourceSegment))
		{
            var entitySet = navigationSource as IEdmEntitySet;
            var singleton = navigationSource as IEdmSingleton;

			tagName = entitySet != null
                ? entitySet.Name + "." + entitySet.EntityType.Name
                : singleton.Name + "." + singleton.EntityType.Name;
        }
		else if (SecondLastSegment is ODataComplexPropertySegment)
		{
            tagName = EdmModelHelper.GenerateComplexPropertyPathTagName(Path, Context);			
        }

		if (tagName != null)
		{
			OpenApiTag tag = new()
			{
				Name = tagName
			};

			if (!IsSingleElement)
				tag.Extensions.Add(Constants.xMsTocType, new OpenApiString("page"));

			operation.Tags.Add(tag);

			Context.AppendTag(tag);
		}		

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
                        Context.CreateSelect(TargetPath, navigationProperty.ToEntityType()) ?? Context.CreateSelect(navigationProperty),
                        Context.CreateExpand(TargetPath, navigationProperty.ToEntityType()) ?? Context.CreateExpand(navigationProperty),
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
                        Context.CreateOrderBy(TargetPath, navigationProperty.ToEntityType()) ?? Context.CreateOrderBy(navigationProperty),
                        Context.CreateSelect(TargetPath, navigationProperty.ToEntityType()) ?? Context.CreateSelect(navigationProperty),
                        Context.CreateExpand(TargetPath, navigationProperty.ToEntityType()) ?? Context.CreateExpand(navigationProperty),
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
                        Context.CreateSelect(TargetPath, entitySet.EntityType) ?? Context.CreateSelect(entitySet),
                        Context.CreateExpand(TargetPath, entitySet.EntityType) ?? Context.CreateExpand(entitySet),
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
                        Context.CreateOrderBy(TargetPath, entitySet.EntityType) ?? Context.CreateOrderBy(entitySet),
                        Context.CreateSelect(TargetPath, entitySet.EntityType) ?? Context.CreateSelect(entitySet),
                        Context.CreateExpand(TargetPath, entitySet.EntityType) ?? Context.CreateExpand(entitySet),
					})
				.Where(x => x != null)
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
		}
		else if(singleton != null)
		{
			new OpenApiParameter[] {
                    Context.CreateSelect(TargetPath, singleton.EntityType) ?? Context.CreateSelect(singleton),
                    Context.CreateExpand(TargetPath, singleton.EntityType) ?? Context.CreateExpand(singleton),
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

        ReadRestrictionsType readRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
        var annotatableReadRestrictions = Context.Model.GetRecord<ReadRestrictionsType>(annotatable, CapabilitiesConstants.ReadRestrictions);
        readRestrictions?.MergePropertiesIfNull(annotatableReadRestrictions);
        readRestrictions ??= annotatableReadRestrictions;

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

    protected override void SetExternalDocs(OpenApiOperation operation)
    {
        if (Context.Settings.ShowExternalDocs && Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) is LinkType externalDocs)
        {
            operation.ExternalDocs = new OpenApiExternalDocs()
            {
                Description = CoreConstants.ExternalDocsDescription,
                Url = externalDocs.Href
            };
        }
    }
}