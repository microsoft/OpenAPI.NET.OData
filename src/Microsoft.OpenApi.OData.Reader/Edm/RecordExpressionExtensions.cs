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

            if (record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmIntegerConstantExpression value = property.Value as IEdmIntegerConstantExpression;
                    if (value != null)
                    {
                        return value.Value;
                    }
                }
            }

            return null;
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

            if (record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmStringConstantExpression value = property.Value as IEdmStringConstantExpression;
                    if (value != null)
                    {
                        return value.Value;
                    }
                }
            }

            return null;
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

            if (record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmBooleanConstantExpression value = property.Value as IEdmBooleanConstantExpression;
                    if (value != null)
                    {
                        return value.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the Enum value from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The output enum type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The Enum value or null.</returns>
        public static T? GetEnum<T>(this IEdmRecordExpression record, string propertyName)
            where T : struct
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmEnumMemberExpression value = property.Value as IEdmEnumMemberExpression;
                    if (value != null && value.EnumMembers != null && value.EnumMembers.Any())
                    {
                        IEdmEnumMember member = value.EnumMembers.First();
                        T result;
                        if (Enum.TryParse(member.Name, out result))
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///  Get the collection of <typeparamref name="T"/> from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="elementAction">The element action.</param>
        /// <returns>The collection or null.</returns>
        public static T GetRecord<T>(this IEdmRecordExpression record, string propertyName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmRecordExpression recordValue = property.Value as IEdmRecordExpression;
                if (recordValue != null)
                {
                    T a = new T();
                    a.Initialize(recordValue);
                    return a;
                }
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

            if (record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmPathExpression value = property.Value as IEdmPathExpression;
                    if (value != null)
                    {
                        return value.Path;
                    }
                }
            }

            return null;
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

            if (record.Properties != null)
            {
                IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
                if (property != null)
                {
                    IEdmCollectionExpression value = property.Value as IEdmCollectionExpression;
                    if (value != null && value.Elements != null)
                    {
                        IList<string> properties = new List<string>();
                        foreach (var path in value.Elements.Select(e => e as IEdmPathExpression))
                        {
                            properties.Add(path.Path);
                        }

                        if (properties.Any())
                        {
                            return properties;
                        }
                    }
                }
            }

            return null;
        }



        /// <summary>
        ///  Get the collection of <typeparamref name="T"/> from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="elementAction">The element action.</param>
        /// <returns>The collection or null.</returns>
        public static IList<T> GetCollection<T>(this IEdmRecordExpression record, string propertyName, Action<T, IEdmRecordExpression> elementAction)
            where T: class, new()
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmCollectionExpression collection = property.Value as IEdmCollectionExpression;
                if (collection != null && collection.Elements != null)
                {
                    IList<T> items = new List<T>();
                    foreach (IEdmRecordExpression item in collection.Elements.OfType<IEdmRecordExpression>())
                    {
                        T a = new T();
                        elementAction(a, item);
                        items.Add(a);
                    }

                    return items;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the collection of <typeparamref name="T"/> from the record using the given property name.
        /// </summary>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="elementFunc">The element func.</param>
        /// <returns>The collection of string or null.</returns>
        public static IEnumerable<T> GetCollection<T>(this IEdmRecordExpression record, string propertyName, Func<IEdmExpression, T> elementFunc)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmCollectionExpression collection = property.Value as IEdmCollectionExpression;
                if (collection != null && collection.Elements != null)
                {
                    return collection.Elements.Select(e => elementFunc(e));
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

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmCollectionExpression collection = property.Value as IEdmCollectionExpression;
                if (collection != null && collection.Elements != null)
                {
                    IList<string> items = new List<string>();
                    foreach (var item in collection.Elements)
                    {
                        IEdmStringConstantExpression itemRecord = item as IEdmStringConstantExpression;
                        items.Add(itemRecord.Value);
                    }

                    return items;
                }
            }

            return null;
        }

        /// <summary>
        ///  Get the collection of <typeparamref name="T"/> from the record using the given property name.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="record">The record expression.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="elementAction">The element action.</param>
        /// <returns>The collection or null.</returns>
        public static IList<T> GetCollection<T>(this IEdmRecordExpression record, string propertyName)
            where T : IRecord, new()
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            IEdmPropertyConstructor property = record.Properties.FirstOrDefault(e => e.Name == propertyName);
            if (property != null)
            {
                IEdmCollectionExpression collection = property.Value as IEdmCollectionExpression;
                if (collection != null && collection.Elements != null)
                {
                    IList<T> items = new List<T>();
                    foreach (IEdmRecordExpression item in collection.Elements.OfType<IEdmRecordExpression>())
                    {
                        T a = new T();
                        a.Initialize(item);
                        items.Add(a);
                    }

                    return items;
                }
            }

            return null;
        }
    }
}
