// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using System;
using System.Diagnostics;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Capabilities
{
    /// <summary>
    /// Extension methods for <see cref="IEdmRecordExpression"/>
    /// </summary>
    internal static class RecordExpressionExtensions
    {
        public static T? GetEnum<T>(this IEdmRecordExpression record, string propertyName)
            where T : struct
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record != null)
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

        public static string GetPropertyPath(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record != null && record.Properties != null)
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

        public static string GetNavigationPropertyPath(this IEdmRecordExpression record, string propertyName)
        {
            Utils.CheckArgumentNull(record, nameof(record));
            Utils.CheckArgumentNull(propertyName, nameof(propertyName));

            if (record != null && record.Properties != null)
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
    }
}
