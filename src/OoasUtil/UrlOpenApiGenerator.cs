//---------------------------------------------------------------------
// <copyright file="UrlOpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi;
using Microsoft.OpenApi.OData;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace OoasUtil
{
    /// <summary>
    /// Open Api generator from Url
    /// </summary>
    internal class UrlOpenApiGenerator : OpenApiGenerator
    {
        /// <summary>
        /// Input of the CSDL
        /// </summary>
        public Uri Input { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlOpenApiGenerator"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="format">The format.</param>
        /// <param name="settings">Conversion settings.</param>
        public UrlOpenApiGenerator(Uri input, string output, OpenApiFormat format, OpenApiConvertSettings settings)
            : base(output, format, settings)
        {
            Input = input;
        }

        /// <summary>
        /// Get the Edm model.
        /// </summary>
        protected override async Task<IEdmModel> GetEdmModelAsync(CancellationToken cancellationToken = default)
        {
            Uri requestUri = new (Input.OriginalString + "/$metadata");

            string csdl = await GetModelDocumentAsync(requestUri, cancellationToken);

            return CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
        }
        private async Task<string> GetModelDocumentAsync(Uri requestUri, CancellationToken cancellationToken) {
            using HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        private static readonly HttpClient client = new ();

        protected override void ModifySettings()
        {
            base.ModifySettings();
            Settings.ServiceRoot = Input;
        }
    }
}
