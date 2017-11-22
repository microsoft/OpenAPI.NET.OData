//---------------------------------------------------------------------
// <copyright file="UrlOpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Net;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi;
using Microsoft.OpenApi.OData;

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
        /// <param name="target">The output target.</param>
        public UrlOpenApiGenerator(Uri input, string output, OpenApiFormat format)
            : base(output, format)
        {
            Input = input;
        }

        /// <summary>
        /// Get the Edm model.
        /// </summary>
        protected override IEdmModel GetEdmModel()
        {
            Uri requestUri = new Uri(Input.OriginalString + "/$metadata");

            WebRequest request = WebRequest.Create(requestUri);

            WebResponse response = request.GetResponse();

            Stream receivedStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(receivedStream, Encoding.UTF8);

            string csdl = reader.ReadToEnd();

            return CsdlReader.Parse(XElement.Parse(csdl).CreateReader());

        }

        protected override OpenApiConvertSettings GetSettings()
        {
            return new OpenApiConvertSettings
            {
                ServiceRoot = Input
            };
        }
    }
}
