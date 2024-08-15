// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Extensions;
using Microsoft.OpenApi.OData.Vocabulary.Core;

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
        /// Get/set the metadata version.
        /// </summary>
        public string SemVerVersion { get; set; } = "1.0.0";

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
        /// Gets/sets a value indicating whether or not to allow the count of a collection of entities.
        /// </summary>
        public bool EnableCount { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether or not to reference @odata.nextLink, @odata.deltaLink and @odata.count in responses
        /// </summary>
        public bool EnableODataAnnotationReferencesForResponses { get; set; } = true;

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
        /// Gets/sets a value indicating whether or not to generate paths with alternate key parameters
        /// </summary>
        public bool AddAlternateKeyPaths { get; set; } = false;

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
        /// Gets/sets links to external documentation for operations
        /// </summary>
        public bool ShowExternalDocs { get; set; } = true;

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
        /// Gets/Sets a value indicating whether or not to retrieve complex or navigation properties declared in derived types.
        /// </summary>
        public bool GenerateDerivedTypesProperties { get; set; } = true;
        
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
        /// Gets/sets a value indicating whether or not to add a "x-ms-enum-flags" extension to the enum type schema.
        /// </summary>
        public bool AddEnumFlagsExtension { get; set; } = true;
        
        /// <summary>
        /// Gets/sets a value indicating whether the error responses should be described as a default response or as 4XX and 5XX error responses.
        /// </summary>
        public bool ErrorResponsesAsDefault { get; set; } = true;

        /// <summary>
        /// Gets/Sets the name of the complex type to look for in the main namespace to use as the inner error type.
        /// </summary>
        public string InnerErrorComplexTypeName { get; set; } = "InnerError";

        /// <summary>
        /// Gets/Sets a value indicating whether path parameters should be declared on path item object.
        /// If true, path parameters will be declared on the path item object, otherwise they 
        /// will be declared on the operation object.
        /// </summary>
        public bool DeclarePathParametersOnPathItem { get; set; } = false;

        /// <summary>
        /// Gets/Sets a value indicating whether or not to use restrictions annotations to generate paths for complex properties.
        /// </summary>
        public bool RequireRestrictionAnnotationsToGenerateComplexPropertyPaths { get; set; } = true;

        /// <summary>
        /// Gets/sets a dictionary containing a mapping of custom attribute names and extension names.
        /// </summary>
        public Dictionary<string, string> CustomXMLAttributesMapping { get; set; } = new();

        /// <summary>
        /// Gets/sets a value indicating whether or not to append bound operations on derived type cast segments.
        /// </summary>
        public bool AppendBoundOperationsOnDerivedTypeCastSegments { get; set; } = false;

        /// <summary>
        /// Gets/Sets a value indicating whether or not to use the HTTP success status code range 2XX
        /// to represent all response codes between 200 - 299.
        /// </summary>
        public bool UseSuccessStatusCodeRange { get; set; } = false;

        /// <summary>
        /// Gets/Sets a value indicating whether to show the version of the assembly used for generating 
        /// Open API document
        /// </summary>
        public bool IncludeAssemblyInfo { get; set; } = true;

        /// <summary>
        /// Get/Sets a dictionary containing a mapping of HTTP methods to custom link relation types 
        /// </summary>
        public Dictionary<LinkRelKey, string> CustomHttpMethodLinkRelMapping { get; set; } = new()
        {
            { LinkRelKey.List, "https://graph.microsoft.com/rels/docs/list" },
            { LinkRelKey.ReadByKey, "https://graph.microsoft.com/rels/docs/get" },
            { LinkRelKey.Create, "https://graph.microsoft.com/rels/docs/create" },
            { LinkRelKey.Update, "https://graph.microsoft.com/rels/docs/update" },
            { LinkRelKey.Delete, "https://graph.microsoft.com/rels/docs/delete" },
            { LinkRelKey.Action, "https://graph.microsoft.com/rels/docs/action" },
            { LinkRelKey.Function, "https://graph.microsoft.com/rels/docs/function" }
        };

        /// <summary>
        /// Gets/sets a value indicating whether to set the default value for a structured type's @odata.type property.
        /// If false, the value will be set conditionally based on whether the type's base type is abstract (and not entity)
        /// and is referenced in the properties of a structural property or an action.
        /// </summary>
        public bool EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty { get; set; } = false;

        /// <summary>
        /// The namespace prefix to be stripped from the in method paths.
        /// </summary>
        public string NamespacePrefixToStripForInMethodPaths { get; set; }

        /// <summary>
        /// Enables the use of aliases for the type cast segments to shorten the url path.
        /// </summary>
        public bool EnableAliasForTypeCastSegments { get; set; } = false;

        /// <summary>
        /// Enables the use of aliases for operation segments to shorten the url path.
        /// </summary>
        public bool EnableAliasForOperationSegments { get; set; } = false;

        /// <summary>
        /// Gets/Sets a value indicating whether or not to generate the schema of query options as an array of string values.
        /// If false, the schema will be generated as an array of enum string values.
        /// </summary>
        public bool UseStringArrayForQueryOptionsSchema { get; set; } = true;

        /// <summary>
        /// Gets/Sets a value indicating the depth to expand composable functions.
        /// </summary>
        public int ComposableFunctionsExpansionDepth { get; set; } = 1;

        internal OpenApiConvertSettings Clone()
        {
            var newSettings = new OpenApiConvertSettings
            {
                ServiceRoot = this.ServiceRoot,
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
                ShowExternalDocs = this.ShowExternalDocs,
                PathProvider = this.PathProvider,
                EnableDollarCountPath = this.EnableDollarCountPath,
                AddSingleQuotesForStringParameters = this.AddSingleQuotesForStringParameters,
                EnableODataTypeCast = this.EnableODataTypeCast,
                RequireDerivedTypesConstraintForODataTypeCastSegments = this.RequireDerivedTypesConstraintForODataTypeCastSegments,
                EnableDeprecationInformation = this.EnableDeprecationInformation,
                AddEnumDescriptionExtension = this.AddEnumDescriptionExtension,
                AddEnumFlagsExtension = this.AddEnumFlagsExtension,
                ErrorResponsesAsDefault = this.ErrorResponsesAsDefault,
                InnerErrorComplexTypeName = this.InnerErrorComplexTypeName,
                RequireRestrictionAnnotationsToGenerateComplexPropertyPaths = this.RequireRestrictionAnnotationsToGenerateComplexPropertyPaths,
                GenerateDerivedTypesProperties = this.GenerateDerivedTypesProperties,
                CustomXMLAttributesMapping = this.CustomXMLAttributesMapping,
                CustomHttpMethodLinkRelMapping = this.CustomHttpMethodLinkRelMapping,
                AppendBoundOperationsOnDerivedTypeCastSegments = this.AppendBoundOperationsOnDerivedTypeCastSegments,
                UseSuccessStatusCodeRange = this.UseSuccessStatusCodeRange,
                EnableCount = this.EnableCount,
                IncludeAssemblyInfo = this.IncludeAssemblyInfo,
                EnableODataAnnotationReferencesForResponses = this.EnableODataAnnotationReferencesForResponses,
                EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty = this.EnableTypeDisambiguationForDefaultValueOfOdataTypeProperty,
                AddAlternateKeyPaths = this.AddAlternateKeyPaths,
                NamespacePrefixToStripForInMethodPaths = this.NamespacePrefixToStripForInMethodPaths,
                EnableAliasForTypeCastSegments = this.EnableAliasForTypeCastSegments,
                SemVerVersion = this.SemVerVersion,
                EnableAliasForOperationSegments = this.EnableAliasForOperationSegments,
                UseStringArrayForQueryOptionsSchema = this.UseStringArrayForQueryOptionsSchema,
                ComposableFunctionsExpansionDepth = this.ComposableFunctionsExpansionDepth
            };

            return newSettings;
        }
    }
}
