// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Extensions;

namespace Microsoft.OpenApi.OData
{
    /// <summary>
    /// Convert settings.
    /// </summary>
    public class OpenApiConvertSettings
    {
        /// <summary>
        /// Gets/sets the service root.
        /// </summary>
        public Uri ServiceRoot { get; set; } = new Uri("http://localhost");

        /// <summary>
        /// Gets/sets the metadata version.
        /// </summary>
        public Version Version { get; set; } = new Version(1, 0, 1);

        /// <summary>
        /// Gets/set a value indicating whether to output key as segment path.
        /// </summary>
        public bool? EnableKeyAsSegment { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output un-qualified operation call.
        /// </summary>
        public bool EnableUnqualifiedCall { get; set; }

        /// <summary>
        /// Gets/set a value indicating whether to output the path for Edm operation.
        /// </summary>
        public bool EnableOperationPath { get; set; } = true;

        /// <summary>
        /// Gets/set a value indicating whether to output the path for Edm operation import.
        /// </summary>
        public bool EnableOperationImportPath { get; set; } = true;

        /// <summary>
        /// Gets/set a value indicating whether to output the path for Edm navigation property.
        /// </summary>
        public bool EnableNavigationPropertyPath { get; set; } = true;

        /// <summary>
        /// Gets/set a value indicating the tags name depth.
        /// </summary>
        public int TagDepth { get; set; } = 4;

        /// <summary>
        /// Gets/set a value indicating whether we prefix entity type name before single key.
        /// </summary>
        public bool PrefixEntityTypeNameBeforeKey { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether the version of openApi to serialize to is v2.
        /// Currently only impacts nullable references for EdmTypeSchemaGenerator
        /// </summary>
        public OpenApiSpecVersion OpenApiSpecVersion { get; set; } = OpenApiSpecVersion.OpenApi3_0;

        /// <summary>
        /// Gets/sets a value indicating to set the OperationId on Open API operation.
        /// </summary>
        public bool EnableOperationId { get; set; } = true;

        /// <summary>
        /// Gets/sets a value indicating whether to output the binding function as Uri escape function if applied the UriEscapeFunction term.
        /// </summary>
        public bool EnableUriEscapeFunctionCall { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether to verify the edm model before converter.
        /// </summary>
        public bool VerifyEdmModel { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether the server is IEEE754 compatible.
        /// If it is IEEE754Compatible, the server will write quoted string for INT64 and decimal to prevent data loss;
        /// otherwise keep number without quotes.
        /// </summary>
        public bool IEEE754Compatible { get; set; }

        /// <summary>
        /// Gets or sets $Top example value.
        /// </summary>
        public int TopExample { get; set; } = 50;

        /// <summary>
        /// Gets/sets a value indicating whether or not to allow paging a collection of entities.
        /// </summary>
        public bool EnablePagination { get; set; }

        /// <summary>
        /// Gets/sets a value that specifies the name of the operation for retrieving the next page in a collection of entities.
        /// </summary>
        public string PageableOperationName { get; set; } = "listMore";

        /// <summary>
        /// Gets/sets a value indicating whether or not to allow discriminator value support.
        /// </summary>
        public bool EnableDiscriminatorValue { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether or not to show the derived types of a base type reference in the responses payload.
        /// </summary>
        public bool EnableDerivedTypesReferencesForResponses { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether or not to show the derived types of a base type reference in the requestBody payload.
        /// </summary>
        public bool EnableDerivedTypesReferencesForRequestBody { get; set; } = false;

        /// <summary>
        /// Gets/sets a value that specifies a prefix to be prepended to all generated paths.
        /// </summary>
        public string PathPrefix
        {
            get
            {
                if (RoutePathPrefixProvider != null)
                {
                    return RoutePathPrefixProvider.PathPrefix;
                }

                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw Error.ArgumentNullOrEmpty("value");
                }

                RoutePathPrefixProvider = new ODataRoutePathPrefixProvider
                {
                    PathPrefix = value
                };
            }
        }

        /// <summary>
        /// Gets/sets a route path prefix provider.
        /// </summary>
        public IODataRoutePathPrefixProvider RoutePathPrefixProvider { get; set; }

        /// <summary>
        /// Gets/Sets a value indicating whether or not to show the OpenAPI links in the responses.
        /// </summary>
        public bool ShowLinks { get; set; } = false;

        /// <summary>
        /// Gets/Sets a value indicating whether or not to show schema examples.
        /// </summary>
        public bool ShowSchemaExamples { get; set; } = false;

        /// <summary>
        /// Gets/Sets a value indicating whether or not to require the
        /// Validation.DerivedTypeConstraint to be applied to NavigationSources
        /// to bind operations of derived types to them.
        /// </summary>
        public bool RequireDerivedTypesConstraintForBoundOperations { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether or not to show the root path of the described API.
        /// </summary>
        public bool ShowRootPath { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether or not to show the group path extension.
        /// </summary>
        public bool ShowMsDosGroupPath { get; set; } = true;

        /// <summary>
        /// Gets/sets a the path provider.
        /// </summary>
        public IODataPathProvider PathProvider { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether or not add OData $count segments in the description for collections.
        /// </summary>
        public bool EnableDollarCountPath { get; set; } = true;

        /// <summary>
        /// Gets/sets a value indicating whether or not single quotes surrounding string parameters in url templates should be added.
        /// </summary>
        public bool AddSingleQuotesForStringParameters { get; set; } = false;
        
        /// <summary>
        /// Gets/sets a value indicating whether or not to include the OData type cast segments.
        /// </summary>
        public bool EnableODataTypeCast { get; set; } = true;

        /// <summary>
        /// Gets/sets a value indicating whether or not to require a derived types constraint to include the OData type cast segments.
        /// </summary>
        public bool RequireDerivedTypesConstraintForODataTypeCastSegments { get; set; } = true;

        /// <summary>
        /// Gets/sets a value indicating whether or not to set the deprecated tag for the operation when a revision is present as well as the "x-ms-deprecation" extension with additional information.
        /// </summary>
        public bool EnableDeprecationInformation { get; set; } = true;

        /// <summary>
        /// Gets/sets a value indicating whether or not to add a "x-ms-enum" extension to the enum type schema for V2 and V3 descriptions.
        /// V3.1 will won't add the extension.
        /// https://github.com/Azure/autorest/blob/main/docs/extensions/readme.md#x-ms-enum
        /// </summary>
        public bool AddEnumDescriptionExtension { get; set; } = false;

        /// <summary>
        /// Gets/sets a value indicating whether the error responses should be described as a default response or as 4XX and 5XX error responses.
        /// </summary>
        public bool ErrorResponsesAsDefault { get; set; } = true;

        /// <summary>
        /// Gets/Sets the name of the complex type to look for in the main namespace to use as the inner error type.
        /// </summary>
        public string InnerErrorComplexTypeName { get; set; } = "InnerError";

        /// <summary>
        /// Gets/Sets a value indicating whether or not to use restrictions annotations to generate paths for complex properties.
        /// </summary>
        public bool UseRestrictionAnnotationsToGeneratePathsForComplexProperties { get; set; } = true;

        internal OpenApiConvertSettings Clone()
        {
            var newSettings = new OpenApiConvertSettings
            {
                ServiceRoot = this.ServiceRoot,
                Version = this.Version,
                EnableKeyAsSegment = this.EnableKeyAsSegment,
                EnableUnqualifiedCall = this.EnableUnqualifiedCall,
                EnableOperationPath = this.EnableOperationPath,
                EnableOperationImportPath = this.EnableOperationImportPath,
                EnableNavigationPropertyPath = this.EnableNavigationPropertyPath,
                TagDepth = this.TagDepth,
                PrefixEntityTypeNameBeforeKey = this.PrefixEntityTypeNameBeforeKey,
                OpenApiSpecVersion = this.OpenApiSpecVersion,
                EnableOperationId = this.EnableOperationId,
                VerifyEdmModel = this.VerifyEdmModel,
                IEEE754Compatible = this.IEEE754Compatible,
                TopExample = this.TopExample,
                EnableUriEscapeFunctionCall = this.EnableUriEscapeFunctionCall,
                EnablePagination = this.EnablePagination,
                PageableOperationName = this.PageableOperationName,
                EnableDiscriminatorValue = this.EnableDiscriminatorValue,
                EnableDerivedTypesReferencesForResponses = this.EnableDerivedTypesReferencesForResponses,
                EnableDerivedTypesReferencesForRequestBody = this.EnableDerivedTypesReferencesForRequestBody,
                RoutePathPrefixProvider = this.RoutePathPrefixProvider,
                ShowLinks = this.ShowLinks,
                RequireDerivedTypesConstraintForBoundOperations = this.RequireDerivedTypesConstraintForBoundOperations,
                ShowSchemaExamples = this.ShowSchemaExamples,
                ShowRootPath = this.ShowRootPath,
                PathProvider = this.PathProvider,
                EnableDollarCountPath = this.EnableDollarCountPath,
                AddSingleQuotesForStringParameters = this.AddSingleQuotesForStringParameters,
                EnableODataTypeCast = this.EnableODataTypeCast,
                RequireDerivedTypesConstraintForODataTypeCastSegments = this.RequireDerivedTypesConstraintForODataTypeCastSegments,
                EnableDeprecationInformation = this.EnableDeprecationInformation,
                AddEnumDescriptionExtension = this.AddEnumDescriptionExtension,
                ErrorResponsesAsDefault = this.ErrorResponsesAsDefault,
                InnerErrorComplexTypeName = this.InnerErrorComplexTypeName,
                UseRestrictionAnnotationsToGeneratePathsForComplexProperties = this.UseRestrictionAnnotationsToGeneratePathsForComplexProperties
            };

            return newSettings;
        }
    }
}
