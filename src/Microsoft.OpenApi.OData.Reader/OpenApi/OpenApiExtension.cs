//---------------------------------------------------------------------
// <copyright file="OpenApiExtension.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using Microsoft.OData.OpenAPI.Properties;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Additional data can be added to extend the specification at certain points.
    /// </summary>
    internal class OpenApiExtension : IOpenApiElement, IOpenApiWritable
    {
        /// <summary>
        /// The field name MUST begin with x-, for example, x-internal-id
        /// </summary>
        public string Name { get;}

        /// <summary>
        /// The value can be null, a primitive, an array or an object. Can have any valid JSON format value.
        /// </summary>
        public object Value { get;}

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiExtension"/> class.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="value">The field value.</param>
        public OpenApiExtension(string name, object value)
        {
            VerifyExtensionName(name);
            VerifyExtensionValue(value);

            Name = name;
            Value = value;
        }

        /// <summary>
        /// Write Open API document to given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            writer.WritePropertyName(Name);

            if (Value is IOpenApiWritable)
            {
                ((IOpenApiWritable)Value).Write(writer);
            }
            else
            {
                // TODO: 
                writer.WriteValue(Value);
            }
        }

        private static void VerifyExtensionName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNullOrEmpty("name");
            }

            if (!name.StartsWith(OpenApiConstants.OpenApiDocExtensionFieldNamePrefix))
            {
                throw new OpenApiException(SRResource.ExtensionFieldNameMustBeginWithXMinus);
            }
        }

        private static void VerifyExtensionValue(object value)
        {
            // TODO, verify the value can be null, a primitive, an array or an object.
            // Can have any valid JSON format value
        }
    }
}
