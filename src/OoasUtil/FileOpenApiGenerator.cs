//---------------------------------------------------------------------
// <copyright file="FileOpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi;

namespace OoasUtil
{
    /// <summary>
    /// Open Api generator from file.
    /// </summary>
    internal class FileOpenApiGenerator : OpenApiGenerator
    {
        /// <summary>
        /// Input of the CSDL
        /// </summary>
        public string Input { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileOpenApiGenerator"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="target">The output target.</param>
        public FileOpenApiGenerator(string input, string output, OpenApiFormat format)
            : base(output, format)
        {
            Input = input;
        }

        /// <summary>
        /// Process the arguments.
        /// </summary>
        protected override IEdmModel GetEdmModel()
        {
            try
            {
                string csdl = File.ReadAllText(Input);
                return CsdlReader.Parse(XElement.Parse(csdl).CreateReader());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
