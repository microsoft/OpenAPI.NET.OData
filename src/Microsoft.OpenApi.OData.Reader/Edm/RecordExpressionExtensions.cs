// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Vocabulary;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Extension methods for <see cref="IEdmRecordExpression"/>
    /// </summary>
    internal static class RecordExpressionExtensions
    {
        /// <summary>
        /// Get the integer value from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The integer value or null.</returns>
        public static long? GetInteger(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            return (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmIntegerConstantExpression value) ?
                value.Value :
                null;
        }

        /// <summary>
        /// Gets the string value of a property in the given record expression.
        /// </summary>
        /// <param name="record">The given record.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property string value.</returns>
        public static string GetString(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            return (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
            property.Value is IEdmStringConstantExpression value) ?
                value.Value :
                null;
        }

        /// <summary>
        /// Get the boolean value from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The boolean value or null.</returns>
        public static bool? GetBoolean(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            return (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmBooleanConstantExpression value) ?
                value.Value :
                null;
        }

        /// <summary>
        /// Get the DateTime value from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The DateTime value or null.</returns>
        public static DateTime? GetDateTime(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            return (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmDateConstantExpression value) ?
                value.Value :
                null;
        }

        /// <summary>
        /// Get the Enum value from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The output enum type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The Enum value or null.</returns>
        public static T? GetEnum<T>(this IEdmRecordExpression record, string propertyName)
            where T : struct, Enum
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal))
                is IEdmPropertyConstructor property &&
                property.Value is IEdmEnumMemberExpression value &&
                value.EnumMembers != null &&
                value.EnumMembers.Any())
            {
                long combinedValue = 0;
                foreach (var enumMember in value.EnumMembers)
                {
                    if (Enum.TryParse(enumMember.Name, out T enumValue))
                    {
                        combinedValue |= Convert.ToInt64(enumValue);
                    }
                }

                return (T)Enum.ToObject(typeof(T), combinedValue);
            }

            return null;
        }

        /// <summary>
        ///  Get the collection of <typeparamref name="T"/> from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The collection or null.</returns>
        public static T GetRecord<T>(this IEdmRecordExpression record, string propertyName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmRecordExpression recordValue)
            {
                T a = new();
                a.Initialize(recordValue);
                return a;
            }

            return default;
        }

        /// <summary>
        /// Get the property path from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property path or null.</returns>
        public static string GetPropertyPath(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            return (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmPathExpression value) ?
                value.Path :
                null;
        }

        /// <summary>
        /// Get the collection of property path from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The collection of property path or null.</returns>
        public static IList<string> GetCollectionPropertyPath(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmCollectionExpression value && value.Elements != null)
            {
                IList<string> properties = 
                    value.Elements
                        .OfType<IEdmPathExpression>()
                        .Select(x => x.Path)
                        .ToList();

                if (properties.Any())
                {
                    return properties;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the collection of string from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The collection of string or null.</returns>
        public static IList<string> GetCollection(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmCollectionExpression collection && collection.Elements != null)
            {
                IList<string> items = collection.Elements
                                    .OfType<IEdmStringConstantExpression>()
                    .Select(x => x.Value)
                    .ToList();
                return items;
            }

            return null;
        }

        /// <summary>
        ///  Get the collection of <typeparamref name="T"/> from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The collection or null.</returns>
        public static IList<T> GetCollection<T>(this IEdmRecordExpression record, string propertyName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record.Properties?.FirstOrDefault(e => propertyName.Equals(e.Name, StringComparison.Ordinal)) is IEdmPropertyConstructor property &&
                property.Value is IEdmCollectionExpression collection && collection.Elements != null)
            {
                IList<T> items = new List<T>();
                foreach (IEdmRecordExpression item in collection.Elements.OfType<IEdmRecordExpression>())
                {
                    T a = new();
                    a.Initialize(item);
                    items.Add(a);
                }

                return items;
            }

            return null;
        }
    }
}
