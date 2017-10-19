//---------------------------------------------------------------------
// <copyright file="EnumerationExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Enumeration type extension methods.
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
        /// <param name="enumVal">The enum value.</param>
        /// <returns>The attribute of type <typeparam name="T"> or null.</returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            Type type = enumVal.GetType();
            MemberInfo memInfo = type.GetMember(enumVal.ToString()).First();
            IEnumerable<T> attributes = memInfo.GetCustomAttributes<T>(false);
            return attributes.FirstOrDefault();
        }
    }
}
