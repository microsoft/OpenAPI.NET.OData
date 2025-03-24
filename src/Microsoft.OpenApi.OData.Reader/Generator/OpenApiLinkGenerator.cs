// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Edm;
using System.Linq;
using Microsoft.OpenApi.Models.Interfaces;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiLink"/> by <see cref="IEdmModel"/>.
    /// </summary>
    internal static class OpenApiLinkGenerator
    {
        /// <summary>
        /// Create the collection of <see cref="OpenApiLink"/> object for an entity or collection of entities.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entityType">The Entity type.</param>
        /// <param name ="entityName">The name of the source of the <see cref="IEdmEntityType"/> object.</param>
        /// <param name="entityKind">"The kind of the source of the <see cref="IEdmEntityType"/> object.</param>
        /// <param name="path">The OData path of the operation the links are to be generated for.</param>
        /// <param name="parameters">"Optional: The list of parameters of the incoming operation.</param>
        /// <param name="navPropOperationId">Optional: The operation id of the source of the NavigationProperty object.</param>
        /// <returns>The created dictionary of <see cref="OpenApiLink"/> object.</returns>
        public static IDictionary<string, IOpenApiLink> CreateLinks(this ODataContext context,
            IEdmEntityType entityType, string entityName, string entityKind, ODataPath path,
            IList<IOpenApiParameter>? parameters = null, string? navPropOperationId = null)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entityType, nameof(entityType));
            Utils.CheckArgumentNullOrEmpty(entityName, nameof(entityName));
            Utils.CheckArgumentNullOrEmpty(entityKind, nameof(entityKind));
            Utils.CheckArgumentNull(path, nameof(path));

            List<string> pathKeyNames = new();

            // Fetch defined Id(s) from incoming parameters (if any)
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    if (!string.IsNullOrEmpty(parameter.Description) &&
                        parameter.Description.ToLower().Contains("key"))
                    {
                        pathKeyNames.Add(parameter.Name);
                    }
                }
            }

            Dictionary<string, IOpenApiLink> links = new();
            bool lastSegmentIsColNavProp = (path.LastSegment as ODataNavigationPropertySegment)?.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.Many;

            // Valid only for non collection-valued navigation properties
            if (!lastSegmentIsColNavProp)
            {
                foreach (IEdmNavigationProperty navProp in entityType.NavigationProperties())
                {
                    string navPropName = navProp.Name;
                    string operationId;
                    string operationPrefix;

                    switch (entityKind)
                    {
                        case "Navigation": // just for contained navigations
                            operationPrefix = navPropOperationId;
                            break;
                        default: // EntitySet, Entity, Singleton
                            operationPrefix = entityName;
                            break;
                    }

                    if (navProp.TargetMultiplicity() == EdmMultiplicity.Many)
                    {
                        operationId = operationPrefix + ".List" + Utils.UpperFirstChar(navPropName);
                    }
                    else
                    {
                        operationId = operationPrefix + ".Get" + Utils.UpperFirstChar(navPropName);
                    }

                    OpenApiLink link = new OpenApiLink
                    {
                        OperationId = operationId,
                        Parameters = new Dictionary<string, RuntimeExpressionAnyWrapper>()
                    };

                    if (pathKeyNames.Any())
                    {
                        foreach (var pathKeyName in pathKeyNames)
                        {
                            link.Parameters[pathKeyName] = new RuntimeExpressionAnyWrapper
                            {
                                Any = "$request.path." + pathKeyName
                            };
                        }
                    }

                    links[navProp.Name] = link;
                }
            }

            // Get the Operations and OperationImport paths bound to this (collection of) entity.
            IEnumerable<ODataPath> operationPaths = context.AllPaths.Where(p => (p.Kind.Equals(ODataPathKind.Operation) || p.Kind.Equals(ODataPathKind.OperationImport)) &&
                path.GetPathItemName().Equals(p.Clone().Pop().GetPathItemName()));

            // Generate links to the Operations and OperationImport operations.
            if (operationPaths.Any())
            {
                foreach (var operationPath in operationPaths)
                {
                    OpenApiLink link = new()
                    {
                        OperationId = string.Join(".", operationPath.Segments.Select(x =>
                        {
                            return x.Kind.Equals(ODataSegmentKind.Key) ? x.EntityType.Name : x.Identifier;
                        }))
                    };

                    links[operationPath.LastSegment.Identifier] = link;
                }
            }

            return links;
        }
    }
}
