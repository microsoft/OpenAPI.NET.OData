//---------------------------------------------------------------------
// <copyright file="OpenApiSchema.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Schema Object.
    /// </summary>
    internal class OpenApiSchema : IOpenApiElement, IOpenApiWritable, IOpenApiReferencable, IOpenApiExtensible
    {
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// MultipleOf
        /// </summary>
        public decimal? MultipleOf { get; set; }

        /// <summary>
        /// Maximumn
        /// </summary>
        public decimal? Maximum { get; set; }

        /// <summary>
        /// ExclusiveMaximum
        /// </summary>
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>
        ///  Minimum
        /// </summary>
        public decimal? Minimum { get; set; }

        /// <summary>
        /// ExclusiveMinimum
        /// </summary>
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>
        /// MaxLength
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// MinLength
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Pattern
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// MaxItems
        /// </summary>
        public int? MaxItems { get; set; }

        /// <summary>
        /// MinItems
        /// </summary>
        public int? MinItems { get; set; }

        /// <summary>
        /// UniqueItems
        /// </summary>
        public bool? UniqueItems { get; set; }

        /// <summary>
        /// MaxProperties
        /// </summary>
        public int? MaxProperties { get; set; }

        /// <summary>
        /// MinProperties
        /// </summary>
        public int? MinProperties { get; set; }

        /// <summary>
        /// Required.
        /// </summary>
        public IList<string> Required { get; set; }

        /// <summary>
        /// Enum
        /// </summary>
        public IList<string> Enum { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// AllOf
        /// </summary>
        public List<OpenApiSchema> AllOf { get; set; }

        /// <summary>
        /// OneOf
        /// </summary>
        public List<OpenApiSchema> OneOf { get; set; }

        /// <summary>
        /// AnyOf
        /// </summary>
        public List<OpenApiSchema> AnyOf { get; set; }

        /// <summary>
        /// Not
        /// </summary>
        public IList<OpenApiSchema> Not { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public OpenApiSchema Items { get; set; }

        /// <summary>
        /// Properties
        /// </summary>
        public IDictionary<string, OpenApiSchema> Properties { get; set; }

        /// <summary>
        /// AdditionalProperties
        /// </summary>
        public OpenApiSchema AdditionalProperties { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Default
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// Allows sending a null value for the defined schema. Default value is false.
        /// </summary>
        public bool? Nullable { get; set; }

        /// <summary>
        /// The discriminator is an object name that is used to differentiate
        /// between other schemas which may satisfy the payload description.
        /// </summary>
        public OpenApiDiscriminator Discriminator { get; set; }

        /// <summary>
        /// Read only.
        /// </summary>
        public bool? ReadOnly { get; set; }

        /// <summary>
        /// Write only.
        /// </summary>
        public bool? WriteOnly { get; set; }

        /// <summary>
        /// Xml object.
        /// </summary>
        public OpenApiXml Xml { get; set; }

        /// <summary>
        /// Additional external documentation for this schema.
        /// </summary>
        public OpenApiExternalDocs ExternalDocs { get; set; }

        /// <summary>
        /// A free-form property to include an example of an instance for this schema.
        /// </summary>
        public OpenApiAny Example { get; set; }

        /// <summary>
        /// Specifies that a schema is deprecated and SHOULD be transitioned out of usage.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Reference Object.
        /// </summary>
        public OpenApiReference Reference { get; set; }

        /// <summary>
        /// Write schema object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (Reference != null)
            {
                Reference.Write(writer);
            }
            else
            {
                WriteInternal(writer);
            }
        }

        private void WriteInternal(IOpenApiWriter writer)
        {
            Debug.Assert(writer != null);

            // { for json, empty for YAML
            writer.WriteStartObject();

            // type
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocType, Type);

            // title
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocTitle, Title);

            // multipleOf
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMultipleOf, MultipleOf);

            // Pattern
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocPattern, Pattern);

            // MaxItems
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMaxItems, MaxItems);

            // MinItems
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMinItems, MinItems);

            // UniqueItems
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocUniqueItems, UniqueItems);

            // MaxProperties
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMaxProperties, MaxProperties);

            // MinProperties
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMinProperties, MinProperties);

            // Required
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocRequired, Required);

            // enum
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocEnum, Enum);

            // AllOf
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocAllOf, AllOf);

            // OneOf
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocOneOf, OneOf);

            // AnyOf
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocAnyOf, AnyOf);

            // Not
            writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocNot, Not);

            // Items
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocItems, Items);

            // Properties
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocProperties, Properties);

            // AdditionalProperties
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocAdditionalProperties, AdditionalProperties);

            // Description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // Format
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocFormat, Format);

            // Maximum
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMaximum, Maximum);

            // exclusiveMaximum
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocExclusiveMaximum, ExclusiveMaximum);

            // Minimum
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMinimum, Minimum);

            // exclusiveMinimum
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocExclusiveMinimum, ExclusiveMinimum);

            // MaxLength
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMaxLength, MaxLength);

            // MaxLength
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocMinLength, MinLength);

            // Default
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDefault, Default);

            // nullable
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocNullable, Nullable);

            // Discriminator
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocDiscriminator, Discriminator);

            // ReadOnly
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocReadOnly, ReadOnly);

            // WriteOnly
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocWriteOnly, WriteOnly);

            // XML
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocXml, Xml);

            // ExternalDocs
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocExternalDocs, ExternalDocs);

            // Example
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocExample, Example);

            // Deprecated
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDeprecated, Deprecated);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
