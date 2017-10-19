//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.IO;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Extension methods to write Entity Data Model (EDM) to Open API.
    /// </summary>
    public static class EdmModelOpenApiExtensions
    {
        /// <summary>
        /// Outputs Edm model to an Open API artifact to the give stream.
        /// </summary>
        /// <param name="model">Edm model to be written.</param>
        /// <param name="stream">The output stream.</param>
        /// <param name="target">The Open API target.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, Stream stream, OpenApiTarget target, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (stream == null)
            {
                throw Error.ArgumentNull(nameof(stream));
            }

            IOpenApiWriter openApiWriter = BuildWriter(stream, target);
            model.WriteOpenApi(openApiWriter, settings);
        }

        /// <summary>
        /// Outputs Edm model to an Open API artifact to the give text writer.
        /// </summary>
        /// <param name="model">Edm model to be written.</param>
        /// <param name="writer">The output text writer.</param>
        /// <param name="target">The Open API target.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, TextWriter writer, OpenApiTarget target, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            IOpenApiWriter openApiWriter = BuildWriter(writer, target);
            model.WriteOpenApi(openApiWriter, settings);
        }

        /// <summary>
        /// Outputs an Open API artifact to the provided Open Api writer.
        /// </summary>
        /// <param name="model">Model to be written.</param>
        /// <param name="writer">The generated Open API writer <see cref="IOpenApiWriter"/>.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, IOpenApiWriter writer, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (settings == null)
            {
                settings = new OpenApiWriterSettings();
            }

            EdmOpenApiDocumentGenerator converter = new EdmOpenApiDocumentGenerator(model, settings);
            OpenApiDocument doc = converter.Generate();
            doc.Write(writer);
        }

        private static IOpenApiWriter BuildWriter(Stream stream, OpenApiTarget target)
        {
            StreamWriter writer = new StreamWriter(stream)
            {
                NewLine = "\n"
            };

            return BuildWriter(writer, target);
        }

        private static IOpenApiWriter BuildWriter(TextWriter writer, OpenApiTarget target)
        {
            if (target == OpenApiTarget.Json)
            {
                return new OpenApiJsonWriter(writer);
            }
            else
            {
                return new OpenApiYamlWriter(writer);
            }
        }
    }
}
