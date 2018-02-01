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

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Extension methods for <see cref="IEdmRecordExpression"/>
    /// </summary>
    internal static class RecordExpressionExtensions
    {
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
                        foreach (var a in value.Elements.Select(e => e as IEdmPathExpression))
                        {
                            properties.Add(a.Path);
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
    }
}
