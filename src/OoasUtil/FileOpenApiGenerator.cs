//---------------------------------------------------------------------
// <copyright file="FileOpenApiGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OpenApi;
using Microsoft.OpenApi.OData;
using System.Threading.Tasks;
using System.Threading;

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
        /// <param name="format">The format.</param>
        /// <param name="settings">Conversion settings.</param>
        public FileOpenApiGenerator(string input, string output, OpenApiFormat format, OpenApiConvertSettings settings)
            : base(output, format, settings)
        {
            Input = input;
        }

        /// <summary>
        /// Process the arguments.
        /// </summary>
        protected override async Task<IEdmModel> GetEdmModelAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                string csdl = await File.ReadAllTextAsync(Input, cancellationToken);
                var directory = Path.GetDirectoryName(Input);
                var parsed = XElement.Parse(csdl);
                using (XmlReader mainReader = parsed.CreateReader())
                {
                    return CsdlReader.Parse(mainReader, u =>
                    {
                        // Currently only support relative paths
                        if (u.IsAbsoluteUri)
                        {
                            Console.WriteLine($"Referenced model must use relative paths to the main model.");
                            return null;
                        }

                        var file = Path.Combine(directory, u.OriginalString);
                        string referenceText = File.ReadAllText(file);
                        var referenceParsed = XElement.Parse(referenceText);
                        XmlReader referenceReader = referenceParsed.CreateReader();
                        return referenceReader;
                    });
                }
            }
            catch (EdmParseException parseException)
            {
                Console.WriteLine("Failed to parse the CSDL file.");
                Console.WriteLine(string.Join(Environment.NewLine, parseException.Errors.Select(e => e.ToString())));
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
