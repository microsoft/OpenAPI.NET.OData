//---------------------------------------------------------------------
// <copyright file="OpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.OData.Edm;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;

namespace OoasUtil
{
    /// <summary>
    /// Open Api generator.
    /// </summary>
    internal abstract class OpenApiGenerator
    {
        /// <summary>
        /// Output format.
        /// </summary>
        public OpenApiFormat Format { get; }

        /// <summary>
        /// Output file.
        /// </summary>
        public string Output { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiGenerator"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="target">The output target.</param>
        public OpenApiGenerator(string output, OpenApiFormat format)
        {
            Output = output;
            Format = format;
        }

        /// <summary>
        /// Generate the Open Api.
        /// </summary>
        public bool Generate()
        {
            try
            {
                IEdmModel edmModel = GetEdmModel();

                OpenApiConvertSettings settings = GetSettings();

                using (FileStream fs = File.Create(Output))
                {
                    OpenApiDocument document = edmModel.ConvertToOpenApi(settings);
                    document.Serialize(fs, OpenApiSpecVersion.OpenApi3_0_0, Format);
                    fs.Flush();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        protected virtual OpenApiConvertSettings GetSettings()
        {
            return new OpenApiConvertSettings();
        }

        protected abstract IEdmModel GetEdmModel();
    }
}
