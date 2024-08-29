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
using System.Threading.Tasks;
using System.Threading;

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
        /// Settings to use for conversion.
        /// </summary>
        public OpenApiConvertSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiGenerator"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="format">The output format.</param>
        /// <param name="settings">Conversion settings.</param>
        protected OpenApiGenerator(string output, OpenApiFormat format, OpenApiConvertSettings settings)
        {
            Output = output;
            Format = format;
            Settings = settings;
        }

        /// <summary>
        /// Generate the Open Api.
        /// </summary>
        public async Task<bool> GenerateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IEdmModel edmModel = await GetEdmModelAsync(cancellationToken);

                this.ModifySettings();

                using (FileStream fs = File.Create(Output))
                {
                    OpenApiDocument document = edmModel.ConvertToOpenApi(Settings);
                    document.Serialize(fs, Settings.OpenApiSpecVersion, Format);
                    await fs.FlushAsync(cancellationToken);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        protected virtual void ModifySettings()
        {
        }

        protected abstract Task<IEdmModel> GetEdmModelAsync(CancellationToken cancellationToken = default);
    }
}
