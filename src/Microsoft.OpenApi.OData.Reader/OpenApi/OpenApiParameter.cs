//---------------------------------------------------------------------
// <copyright file="OpenApiParameter.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.OData.OpenAPI.Properties;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Parameter Locations.
    /// A unique parameter is defined by a combination of a name and location.
    /// </summary>
    internal enum ParameterLocation
    {
        /// <summary>
        /// Parameters that are appended to the URL.
        /// </summary>
        query,

        /// <summary>
        /// Custom headers that are expected as part of the request.
        /// </summary>
        header,

        /// <summary>
        /// Used together with Path Templating, where the parameter value is actually part of the operation's URL.
        /// </summary>
        path,

        /// <summary>
        /// Used to pass a specific cookie value to the API.
        /// </summary>
        cookie
    }

    /// <summary>
    /// The type of the parameter value.
    /// </summary>
    internal enum ParameterStyle
    {
        /// <summary>
        /// Path-style parameters.
        /// </summary>
        matrix,

        /// <summary>
        /// Label style parameters.
        /// </summary>
        label,

        /// <summary>
        /// Form style parameters.
        /// </summary>
        form,

        /// <summary>
        /// Simple style parameters.
        /// </summary>
        simple,

        /// <summary>
        /// Space separated array values.
        /// </summary>
        spaceDelimited,

        /// <summary>
        /// Pipe separated array values. 
        /// </summary>
        pipeDelimited,

        /// <summary>
        /// Provides a simple way of rendering nested objects using form parameters.
        /// </summary>
        deepObject
    }

    /// <summary>
    /// Parameter Object.
    /// </summary>
    internal class OpenApiParameter : IOpenApiElement, IOpenApiExtensible, IOpenApiWritable, IOpenApiReferencable
    {
        private bool _required = false;
        private OpenApiAny _example;
        private ParameterLocation _location;
        private IDictionary<string, OpenApiExample> _examples;

        /// <summary>
        /// REQUIRED. The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// REQUIRED. The location of the parameter.
        /// </summary>
        public ParameterLocation In
        {
            get
            {
                return _location;
            }
            set
            {
                // Default values for the Style property:
                switch (value)
                {
                    case ParameterLocation.query:
                    case ParameterLocation.cookie:
                        Style = ParameterStyle.form;
                        break;
                    case ParameterLocation.path:
                    case ParameterLocation.header:
                        Style = ParameterStyle.simple;
                        break;
                }

                _location = value;
            }
        }

        /// <summary>
        /// A brief description of the parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Determines whether this parameter is mandatory.
        /// If the parameter location is "path", this property is REQUIRED and its value MUST be true.
        /// Otherwise, the property MAY be included and its default value is false.
        /// </summary>
        public bool Required
        {
            get
            {
                return _required;
            }
            set
            {
                if (In == ParameterLocation.path && !value)
                {
                    throw new OpenApiException(SRResource.OpenApiParameterRequiredPropertyMandatory);
                }

                _required = value;
            }
        }

        /// <summary>
        /// Specifies that a parameter is deprecated and SHOULD be transitioned out of usage.
        /// </summary>
        public bool Deprecated { get; set; }

        /// <summary>
        /// Sets the ability to pass empty-valued parameters.
        /// </summary>
        public bool AllowEmptyValue { get; set; }

        /// <summary>
        /// Describes how the parameter value will be serialized depending on the type of the parameter value.
        /// </summary>
        public ParameterStyle Style { get; set; }

        /// <summary>
        /// When this is true, parameter values of type array or object generate separate parameters for
        /// each value of the array or key-value pair of the map. For other types of parameters
        /// this property has no effect. When style is form, the default value is true.
        /// For all other styles, the default value is false.
        /// </summary>
        public bool Explode { get; set; }

        /// <summary>
        /// Determines whether the parameter value SHOULD allow reserved characters, as defined by RFC3986.
        /// </summary>
        public bool AllowReserved { get; set; }

        /// <summary>
        /// The schema defining the type used for the parameter.
        /// </summary>
        public OpenApiSchema Schema { get; set; }

        /// <summary>
        /// Example of the media type. The example object is mutually exclusive of the examples object.
        /// </summary>
        public OpenApiAny Example
        {
            get
            {
                return _example;
            }
            set
            {
                // The examples object is mutually exclusive of the example object.
                _examples = null;
                _example = value;
            }
        }

        /// <summary>
        /// Examples of the media type. The examples object is mutually exclusive of the example object.
        /// </summary>
        public IDictionary<string, OpenApiExample> Examples
        {
            get
            {
                return _examples;
            }
            set
            {
                // The examples object is mutually exclusive of the example object.
                _example = null;
                _examples = value;
            }
        }

        /// <summary>
        /// A map containing the representations for the parameter.
        /// The key is the media type and the value describes it.
        /// The map MUST only contain one entry.
        /// </summary>
        public IDictionary<string, OpenApiMediaType> Content { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }

        /// <summary>
        /// Reference Object.
        /// </summary>
        public OpenApiReference Reference { get; set; }

        /// <summary>
        /// Write parameter object to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
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

            // name
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocName, Name);

            // in
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocIn, In.ToString());

            // description
            writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocDescription, Description);

            // required
            if (In == ParameterLocation.path)
            {
                writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocRequired, true);
            }
            else
            {
                writer.WriteBooleanProperty(OpenApiConstants.OpenApiDocRequired, Required, false);
            }

            // deprecated
            writer.WriteBooleanProperty(OpenApiConstants.OpenApiDocDeprecated, Deprecated, false);

            // allowEmptyValue
            writer.WriteBooleanProperty(OpenApiConstants.OpenApiDocDeprecated, AllowEmptyValue, false);

            // style
            writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocStyle, Style.ToString());

            // explode
            writer.WriteBooleanProperty(OpenApiConstants.OpenApiDocExplode, Explode, false);

            // allowReserved
            writer.WriteBooleanProperty(OpenApiConstants.OpenApiDocAllowReserved, AllowReserved, false);

            // schema
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocSchema, Schema);

            // example
            writer.WriteOptionalObject(OpenApiConstants.OpenApiDocExample, Example);

            // examples
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocExamples, Examples);

            // content
            writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocContent, Content);

            // specification extensions
            writer.WriteDictionary(Extensions);

            // } for json, empty for YAML
            writer.WriteEndObject();
        }
    }
}
