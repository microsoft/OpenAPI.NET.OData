// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
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
	/// <summary>
	/// Initializes a new instance of <see cref="ODataTypeCastGetOperationHandler"/> class.
	/// </summary>
	/// <param name="document">The document to use to lookup references.</param>
	public ODataTypeCastGetOperationHandler(OpenApiDocument document):base(document)
	{
		
	}
	/// <inheritdoc/>
	public override HttpMethod OperationType => HttpMethod.Get;

	/// <summary>
	/// Gets/sets the segment before cast.
	/// this segment could be "entity set", "Collection property", etc.
	/// </summary>
	internal ODataSegment? SecondLastSegment { get; set; }

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

	private NavigationPropertyRestriction? restriction;
	private IEdmSingleton? singleton;
	private IEdmEntitySet? entitySet;
	private IEdmNavigationProperty? navigationProperty;
	private IEdmStructuredType? parentStructuredType;
	private IEdmSchemaElement? ParentSchemaElement => parentStructuredType as IEdmSchemaElement;
	private IEdmStructuredType? targetStructuredType;
	private IEdmSchemaElement? TargetSchemaElement => targetStructuredType as IEdmSchemaElement;
	private const int SecondLastSegmentIndex = 2;
	private bool isIndexedCollValuedNavProp = false;
	private IEdmNavigationSource? navigationSource;
	private IEdmVocabularyAnnotatable? annotatable;

    /// <inheritdoc/>
    protected override void Initialize(ODataContext context, ODataPath path)
	{
		// resetting the fields as we're reusing the handler
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
			SecondLastSegment = path.Segments[count - SecondLastSegmentIndex];

		parentStructuredType = SecondLastSegment is ODataComplexPropertySegment complexSegment ? complexSegment.ComplexType : SecondLastSegment?.EntityType;
		if (path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment)
		{
        	navigationSource = navigationSourceSegment.NavigationSource;
		}

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
			var thirdLastSegment = path.Segments[count - SecondLastSegmentIndex - 1];
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

		if(path.FirstSegment is ODataNavigationSourceSegment navigationSourceSegment &&
			Context is not null)
		{
			NavigationRestrictionsType? navigation = navigationSourceSegment.NavigationSource switch {
				IEdmEntitySet eSet => Context.Model.GetRecord<NavigationRestrictionsType>(eSet, CapabilitiesConstants.NavigationRestrictions),
				IEdmSingleton single => Context.Model.GetRecord<NavigationRestrictionsType>(single, CapabilitiesConstants.NavigationRestrictions),
				_ => null
			};

			if (navigation?.RestrictedProperties != null && Path is not null)
			{
				var navigationPropertyPath = string.Join("/",
								Path.Segments.Where(s => !(s is ODataKeySegment || s is ODataNavigationSourceSegment
									|| s is ODataStreamContentSegment || s is ODataStreamPropertySegment)).Select(e => e.Identifier));
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

        SetRestrictionFromAnnotatable();
    }
	    

	private void SetRestrictionFromAnnotatable()
	{
		if (annotatable == null)
			return;

		var navigation = Context?.Model.GetRecord<NavigationRestrictionsType>(annotatable, CapabilitiesConstants.NavigationRestrictions);
		if (navigation?.RestrictedProperties != null)
		{
			restriction = navigation.RestrictedProperties.FirstOrDefault(r => r.NavigationProperty == null);
		}
	}

	/// <inheritdoc/>
	protected override void SetBasicInfo(OpenApiOperation operation)
	{
        var _readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context?.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);

        // Summary
        string placeHolder = IsSingleElement 
			? $"Get the item of type {ParentSchemaElement.ShortQualifiedName()} as {TargetSchemaElement.ShortQualifiedName()}" 
			: $"Get the items of type {TargetSchemaElement.ShortQualifiedName()} in the {ParentSchemaElement.ShortQualifiedName()} collection";
        operation.Summary = _readRestrictions?.Description ?? placeHolder;
        operation.Description = _readRestrictions?.LongDescription;

        // OperationId
        if (Context is { Settings.EnableOperationId: true } &&
			Path is not null &&
			TargetSchemaElement is not null)
			operation.OperationId = EdmModelHelper.GenerateODataTypeCastPathOperationIdPrefix(Path, Context) + $".As{Utils.UpperFirstChar(TargetSchemaElement.Name)}-{Path.GetPathHash(Context.Settings)}";

        base.SetBasicInfo(operation);
	}

	/// <inheritdoc/>
	protected override void SetResponses(OpenApiOperation operation)
    {
        if (IsSingleElement)
		{
            IOpenApiSchema? schema = null;

            if (Context is { Settings.EnableDerivedTypesReferencesForResponses: true } && targetStructuredType is not null)
            {
                schema = EdmModelHelper.GetDerivedTypesReferenceSchema(targetStructuredType, Context.Model, _document);
            }

            if (schema == null)
            {
                schema = new OpenApiSchemaReference(TargetSchemaElement.FullName(), _document);
            }

            SetSingleResponse(operation, schema);
        }
		else
		{
            SetCollectionResponse(operation, TargetSchemaElement.FullName());
        }			

		if (Context is not null)
			operation.AddErrorResponses(Context.Settings, _document, false);

		base.SetResponses(operation);
	}

	/// <inheritdoc/>
	protected override void SetTags(OpenApiOperation operation)
	{
		if (Context is null)
			return;
        string? tagName = null;
        operation.Tags ??= new HashSet<OpenApiTagReference>();

        if ((SecondLastSegment is ODataNavigationPropertySegment || isIndexedCollValuedNavProp) &&
			Path is not null)
		{
			tagName = EdmModelHelper.GenerateNavigationPropertyPathTagName(Path, Context);
		}
		else if ((SecondLastSegment is ODataKeySegment && !isIndexedCollValuedNavProp)
				|| (SecondLastSegment is ODataNavigationSourceSegment))
		{
            tagName = navigationSource switch {
				IEdmEntitySet entitySetNavigationSource => entitySetNavigationSource.Name + "." + entitySetNavigationSource.EntityType.Name,
                IEdmSingleton singletonNavigationSource => singletonNavigationSource.Name + "." + singletonNavigationSource.EntityType.Name,
				_ => null
			};
        }
		else if (SecondLastSegment is ODataComplexPropertySegment && Path is not null)
		{
            tagName = EdmModelHelper.GenerateComplexPropertyPathTagName(Path, Context);			
        }

		if (tagName != null)
		{
			if (IsSingleElement)
				Context.AppendTag(new OpenApiTag() { Name = tagName });
			else
				Context.AddExtensionToTag(tagName, Constants.xMsTocType, new JsonNodeExtension("page"), () => new OpenApiTag()
				{
					Name = tagName
				});
			operation.Tags.Add(new OpenApiTagReference(tagName, _document));
		}		

		base.SetTags(operation);
	}
	/// <inheritdoc/>
	protected override void SetParameters(OpenApiOperation operation)
	{
		base.SetParameters(operation);

		if (Context is null)
			return;

		operation.Parameters ??= [];

		if(navigationProperty != null) {
			if (IsSingleElement)
			{
				new IOpenApiParameter?[] {
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSelect(TargetPath, navigationProperty.ToEntityType())) ?? Context.CreateSelect(navigationProperty),
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateExpand(TargetPath, navigationProperty.ToEntityType())) ?? Context.CreateExpand(navigationProperty),
					}
				.OfType<IOpenApiParameter>()
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
			else
			{
				GetParametersForAnnotableOfMany(navigationProperty)
				.Union(
					[
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateOrderBy(TargetPath, navigationProperty.ToEntityType())) ?? Context.CreateOrderBy(navigationProperty),
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSelect(TargetPath, navigationProperty.ToEntityType())) ?? Context.CreateSelect(navigationProperty),
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateExpand(TargetPath, navigationProperty.ToEntityType())) ?? Context.CreateExpand(navigationProperty),
					])
				.OfType<IOpenApiParameter>()
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
		}
		else if(entitySet != null)
		{
			if(IsSingleElement)
			{
				new IOpenApiParameter?[] {
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSelect(TargetPath, entitySet.EntityType)) ?? Context.CreateSelect(entitySet),
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateExpand(TargetPath, entitySet.EntityType)) ?? Context.CreateExpand(entitySet),
					}
				.OfType<IOpenApiParameter>()
				.ToList()
				.ForEach(operation.Parameters.Add);
			}
			else
			{
				GetParametersForAnnotableOfMany(entitySet)
				.Union(
					[
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateOrderBy(TargetPath, entitySet.EntityType)) ?? Context.CreateOrderBy(entitySet),
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSelect(TargetPath, entitySet.EntityType)) ?? Context.CreateSelect(entitySet),
                        (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateExpand(TargetPath, entitySet.EntityType)) ?? Context.CreateExpand(entitySet),
					])
				.OfType<IOpenApiParameter>()
				.ToList()
				.ForEach(p => operation.Parameters.Add(p));
			}
		}
		else if(singleton != null)
		{
			new IOpenApiParameter?[] {
                    (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateSelect(TargetPath, singleton.EntityType)) ?? Context.CreateSelect(singleton),
                    (string.IsNullOrEmpty(TargetPath) ? null : Context.CreateExpand(TargetPath, singleton.EntityType)) ?? Context.CreateExpand(singleton),
				}
			.OfType<IOpenApiParameter>()
			.ToList()
			.ForEach(p => operation.Parameters.Add(p));
		}
	}
	private IEnumerable<IOpenApiParameter?> GetParametersForAnnotableOfMany(IEdmVocabularyAnnotatable annotable) 
	{
		if (Context is null)
			return [];
		// Need to verify that TopSupported or others should be applied to navigation source.
		// So, how about for the navigation property.
		return [
            Context.CreateTop(annotable, _document),
			Context.CreateSkip(annotable, _document),
			Context.CreateSearch(annotable, _document),
			Context.CreateFilter(annotable, _document),
			Context.CreateCount(annotable, _document),
		];
	}

	protected override void SetSecurity(OpenApiOperation operation)
	{
		if (restriction is not {ReadRestrictions.Permissions: not null})
		{
			return;
		}

		operation.Security = Context?.CreateSecurityRequirements(restriction.ReadRestrictions.Permissions, _document).ToList();
	}

	protected override void SetExtensions(OpenApiOperation operation)
	{
		if (Context is { Settings.EnablePagination: true } && !IsSingleElement)
		{
			JsonObject extension = new()
			{
				{ "nextLinkName", "@odata.nextLink"},
				{ "operationName", Context.Settings.PageableOperationName}
			};

			operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
			operation.Extensions.Add(Constants.xMsPageable, new JsonNodeExtension(extension));
		}

		base.SetExtensions(operation);
	}

    protected override void AppendCustomParameters(OpenApiOperation operation)
    {
		if (annotatable == null)
			return;

		if (Context is null)
			return;

        var readRestrictions = string.IsNullOrEmpty(TargetPath) ? null : Context.Model.GetRecord<ReadRestrictionsType>(TargetPath, CapabilitiesConstants.ReadRestrictions);
        if (Context.Model.GetRecord<ReadRestrictionsType>(annotatable, CapabilitiesConstants.ReadRestrictions) is {} annotatableReadRestrictions)
		{
        	readRestrictions?.MergePropertiesIfNull(annotatableReadRestrictions);
        	readRestrictions ??= annotatableReadRestrictions;
		}

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
        if (Context is { Settings.ShowExternalDocs: true } &&
			!string.IsNullOrEmpty(CustomLinkRel) &&
			!string.IsNullOrEmpty(TargetPath) &&
			Context.Model.GetLinkRecord(TargetPath, CustomLinkRel) is LinkType externalDocs)
        {
            operation.ExternalDocs = new OpenApiExternalDocs()
            {
                Description = CoreConstants.ExternalDocsDescription,
                Url = externalDocs.Href
            };
        }
    }
}