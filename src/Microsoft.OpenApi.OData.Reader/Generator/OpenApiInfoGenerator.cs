// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create <see cref="OpenApiInfo"/> by Edm model.
    /// </summary>
    internal static class OpenApiInfoGenerator
    {
        /// <summary>
        /// Create a <see cref="OpenApiInfo"/> object.
        /// </summary>
        /// <param name="context">The OData to Open API context.</param>
        /// <returns>The created <see cref="OpenApiInfo"/> object.</returns>
        public static OpenApiInfo CreateInfo(this ODataContext context)
        {
            Utils.CheckArgumentNull(context, nameof(context));

            // The value of info is an Info Object,
            // It contains the fields title and version, and optionally the field description.
            return new OpenApiInfo
            {
                Title = context.GetTitle(),
                Version = context.GetVersion(),
                Description = context.GetDescription(),
                Extensions = context.GetExtensions()
            };
        }

        private static string GetTitle(this ODataContext context)
        {
            Debug.Assert(context != null);

            // The value of title is the value of the unqualified annotation Core.Description
            // of the main schema or the entity container of the OData service.
            // TODO: https://github.com/Microsoft/OpenAPI.NET.OData/issues/2

            //  or the entity container
            if (context.Model.EntityContainer != null)
            {
                string longDescription = context.Model.GetDescriptionAnnotation(context.Model.EntityContainer);
                if (longDescription != null)
                {
                    return longDescription;
                }
            }

            // If no Core.Description is present, a default title has to be provided as this is a required OpenAPI field.
            string namespaceName = context.Model.EntityContainer != null ?
                context.Model.EntityContainer.Namespace :
                context.Model.DeclaredNamespaces.FirstOrDefault();

            return "OData Service for namespace " + namespaceName;
        }

        private static string GetVersion(this ODataContext context)
        {
            Debug.Assert(context != null);

            // The value of version is the value of the annotation Core.SchemaVersion of the main schema.
            // If no Core.SchemaVersion is present, a default version has to be provided as this is a required OpenAPI field.
            // TODO: https://github.com/Microsoft/OpenAPI.NET.OData/issues/2

            return context.Settings.SemVerVersion;
        }

        private static string GetDescription(this ODataContext context)
        {
            Debug.Assert(context != null);

            // The value of description is the value of the annotation Core.LongDescription of the main schema.
            // TODO: https://github.com/Microsoft/OpenAPI.NET.OData/issues/2

            //  or the entity container
            if (context.EntityContainer != null)
            {
                string longDescription = context.Model.GetLongDescriptionAnnotation(context.Model.EntityContainer);
                if (longDescription != null)
                {
                    return longDescription;
                }
            }

            // so a default description should be provided if no Core.LongDescription annotation is present.
            return "This OData service is located at " + context.Settings.ServiceRoot.OriginalString;
        }

        private static Dictionary<string, IOpenApiExtension> GetExtensions(this ODataContext context)
        {
            Debug.Assert(context != null);

            if (context.Settings.IncludeAssemblyInfo)
            {
                return new Dictionary<string, IOpenApiExtension>()
                {
                    {
                        "x-ms-generated-by",
                        new OpenApiObject
                        {
                            { "toolName", new OpenApiString("Microsoft.OpenApi.OData") },
                            { "toolVersion", new OpenApiString(Assembly.GetExecutingAssembly().GetName().Version.ToString()) }
                        }
                    }
                };
            }
            return null;
        }
    }
}
