//---------------------------------------------------------------------
// <copyright file="OpenApiWriterExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Extension methods for writing Open API documentation.
    /// </summary>
    internal static class OpenApiWriterExtensions
    {
        public static void WriteRequiredProperty(this IOpenApiWriter writer, string name, string value)
        {
            writer.WritePropertyInternal(name, value);
        }

        public static void WriteOptionalProperty(this IOpenApiWriter writer, string name, string element)
        {
            if (element == null)
            {
                return;
            }

            writer.WritePropertyInternal(name, element);
        }

        public static void WriteRequiredProperty<T>(this IOpenApiWriter writer, string name, T value)
            where T: struct
        {
            writer.WritePropertyInternal(name, value);
        }

        public static void WriteRequiredProperty<T>(this IOpenApiWriter writer, string name, T? value)
            where T : struct
        {
            writer.WritePropertyInternal(name, value);
        }

        public static void WriteOptionalProperty<T>(this IOpenApiWriter writer, string name, T? value)
            where T : struct
        {
            if (value == null)
            {
                return;
            }

            writer.WritePropertyInternal(name, value.Value);
        }

        /// <summary>
        /// Write the single of Open API element.
        /// </summary>
        /// <param name="writer">The Open API writer.</param>
        /// <param name="name">The property name.</param>
        /// <param name="value">The Open API element.</param>
        public static void WriteRequiredObject(this IOpenApiWriter writer, string name, IOpenApiWritable value)
        {
            writer.WritePropertyInternal(name, value);
        }

        /// <summary>
        /// Write the single of Open API element if the element is not null, otherwise skip it.
        /// </summary>
        /// <param name="writer">The Open API writer.</param>
        /// <param name="name">The property name.</param>
        /// <param name="element">The Open API element.</param>
        public static void WriteOptionalObject(this IOpenApiWriter writer, string name, IOpenApiWritable element)
        {
            if (element == null)
            {
                return;
            }

            writer.WritePropertyInternal(name, element);
        }

        /// <summary>
        /// Write the collection of Open API element.
        /// </summary>
        /// <param name="writer">The Open API writer.</param>
        /// <param name="name">The property name.</param>
        /// <param name="elements">The collection of Open API element.</param>
        public static void WriteRequiredCollection(this IOpenApiWriter writer, string name, IEnumerable<IOpenApiWritable> elements)
        {
            writer.WriteCollectionInternal(name, elements);
        }

        /// <summary>
        /// Write the collection of Open API element optional.
        /// </summary>
        /// <param name="writer">The Open API writer.</param>
        /// <param name="name">The property name.</param>
        /// <param name="elements">The collection of Open API element.</param>
        public static void WriteOptionalCollection(this IOpenApiWriter writer, string name, IEnumerable<IOpenApiWritable> elements)
        {
            if (elements == null)
            {
                return;
            }

            writer.WriteCollectionInternal(name, elements);
        }

        public static void WriteRequiredCollection(this IOpenApiWriter writer, string name, IEnumerable<string> elements)
        {
            writer.WriteCollectionInternal(name, elements);
        }

        public static void WriteOptionalCollection(this IOpenApiWriter writer, string name, IEnumerable<string> elements)
        {
            if (elements == null)
            {
                return;
            }

            writer.WriteCollectionInternal(name, elements);
        }

        public static void WriteRequiredDictionary<T>(this IOpenApiWriter writer, string name, IDictionary<string, T> elements)
            where T : IOpenApiWritable
        {
            writer.WriteDictionaryInternal(name, elements);
        }

        public static void WriteOptionalDictionary<T>(this IOpenApiWriter writer, string name, IDictionary<string, T> elements)
            where T : IOpenApiWritable
        {
            if (elements == null)
            {
                return;
            }

            writer.WriteDictionaryInternal(name, elements);
        }

        public static void WriteRequiredDictionary(this IOpenApiWriter writer, string name, IDictionary<string, string> elements)
        {
            writer.WriteDictionaryInternal(name, elements);
        }

        public static void WriteOptionalDictionary(this IOpenApiWriter writer, string name, IDictionary<string, string> elements)
        {
            if (elements == null)
            {
                return;
            }

            writer.WriteDictionaryInternal(name, elements);
        }

        public static void WriteDictionary(this IOpenApiWriter writer, IEnumerable<IOpenApiWritable> elements)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (elements == null)
            {
                return;
            }

            foreach (IOpenApiWritable e in elements)
            {
                e.Write(writer);
            }
        }

        /// <summary>
        /// Write the boolean property.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <param name="defaultValue">The default value.</param>
        public static void WriteBooleanProperty(this IOpenApiWriter writer, string name, bool value, bool? defaultValue)
        {
            if (defaultValue != null && value == defaultValue.Value)
            {
                return;
            }

            writer.WriteProperty(name, () => writer.WriteValue(value));
        }

        public static void WriteSingleProperty<T>(this IOpenApiWriter writer, string name, T value, T defaultValue = default(T), bool optional = true)
        {
            if (value.Equals(defaultValue))
            {
                if (!optional)
                {
                    return;
                }
            }

            writer.WritePropertyInternal(name, value);
        }


        public static void WriteSingleProperty(this IOpenApiWriter writer, string name, string value, bool optional = true)
        {
            if (value == null && !optional)
            {
                return;
            }

            writer.WritePropertyInternal(name, value);
        }

        public static void WriteIntegerProperty(this IOpenApiWriter writer, string name, int? value, bool optional = true)
        {
            if (value == null && !optional)
            {
                return;
            }
        }

        /// <summary>
        /// Write property with an action.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="name">The property name.</param>
        /// <param name="valueAction">The value action.</param>
        public static void WriteProperty(this IOpenApiWriter writer, string name,
            Action valueAction)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNullOrEmpty("name");
            }

            if (valueAction == null)
            {
                return;
            }

            writer.WritePropertyName(name);
            valueAction();
        }

        /// <summary>
        /// Write an object with an action.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="objectAction">The object action.</param>
        public static void WriteObject(this IOpenApiWriter writer, Action objectAction)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (objectAction == null)
            {
                throw Error.ArgumentNull("valueAction");
            }

            writer.WriteStartObject();

            objectAction();

            writer.WriteEndObject();
        }

        /// <summary>
        /// Write an array with an action.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="arrayAction">The array action.</param>
        public static void WriteArray(this IOpenApiWriter writer, Action arrayAction)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (arrayAction == null)
            {
                throw Error.ArgumentNull("valueAction");
            }

            writer.WriteStartArray();
            arrayAction();
            writer.WriteEndArray();
        }

        private static void WritePropertyInternal<T>(this IOpenApiWriter writer, string name, T element)
        {
            CheckArgument(writer, name);

            writer.WritePropertyName(name);
            writer.WriteValueInternal(element);
        }

        private static void WriteCollectionInternal<T>(this IOpenApiWriter writer, string name, IEnumerable<T> elements)
        {
            CheckArgument(writer, name);

            writer.WritePropertyName(name);

            writer.WriteStartArray();

            if (elements != null)
            {
                foreach (T e in elements)
                {
                    writer.WriteValueInternal(e);
                }
            }

            writer.WriteEndArray();
        }

        private static void WriteDictionaryInternal<T>(this IOpenApiWriter writer, string name, IDictionary<string, T> elements)
        {
            CheckArgument(writer, name);

            writer.WritePropertyName(name);

            writer.WriteStartObject();

            if (elements != null)
            {
                foreach (KeyValuePair<string, T> e in elements)
                {
                    writer.WritePropertyName(e.Key);
                    writer.WriteValueInternal(e.Value);
                }
            }

            writer.WriteEndObject();
        }

        private static void WriteValueInternal<T>(this IOpenApiWriter writer, T value)
        {
            IOpenApiWritable writableElement = value as IOpenApiWritable;
            if (writableElement != null)
            {
                writableElement.Write(writer);
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        private static void CheckArgument(IOpenApiWriter writer, string name)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw Error.ArgumentNullOrEmpty(nameof(name));
            }
        }
    }
}
