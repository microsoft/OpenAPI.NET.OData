// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;
using Microsoft.OpenApi.OData.Edm;

namespace Microsoft.OpenApi.OData.Generator
{
    /// <summary>
    /// Extension methods to create path item name for Edm elements.
    /// </summary>
    internal static class PathItemNameExtensions
    {
        /// <summary>
        /// Create the path item name for the entity from <see cref="IEdmEntitySet"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The created path item name.</returns>
        public static string CreateEntityPathName(this ODataContext context, IEdmEntitySet entitySet)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(entitySet, nameof(entitySet));

            StringBuilder sb = new StringBuilder("/" + entitySet.Name);
            IList<IEdmStructuralProperty> keys = entitySet.EntityType().Key().ToList();
            if (keys.Count() == 1)
            {
                if (context.KeyAsSegment)
                {
                    sb.Append("/{").Append(keys.First().Name).Append("}");
                }
                else
                {
                    sb.Append("('{").Append(keys.First().Name).Append("}')");
                }
            }
            else
            {
                IList<string> keyStrings = new List<string>();
                foreach (var keyProperty in entitySet.EntityType().Key())
                {
                    keyStrings.Add(keyProperty.Name + "={" + keyProperty.Name + "}");
                }
                sb.Append("('").Append(String.Join(",", keyStrings)).Append("')");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create the path item name for a <see cref="IEdmOperationImport"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operationImport">The Edm operation import.</param>
        /// <returns>The created path item name</returns>
        public static string CreatePathItemName(this ODataContext context, IEdmOperationImport operationImport)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(operationImport, nameof(operationImport));

            if (operationImport.IsActionImport())
            {
                return "/" + operationImport.Name;
            }
            else
            {
                StringBuilder functionName = new StringBuilder("/" + operationImport.Name + "(");
                functionName.Append(String.Join(",",
                    operationImport.Operation.Parameters.Select(p =>
                    {
                        if (p.Type.IsStructured() || p.Type.IsCollection())
                        {
                            return p.Name + "=@" + p.Name;
                        }
                        else
                        {
                            return p.Name + "={" + p.Name + "}";
                        }
                    })));

                functionName.Append(")");

                return functionName.ToString();
            }
        }

        /// <summary>
        /// Create the path item name for a <see cref="IEdmOperation"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="operation">The Edm operation.</param>
        /// <returns>The created path item name</returns>
        public static string CreatePathItemName(this ODataContext context, IEdmOperation operation)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(operation, nameof(operation));

            if (operation.IsAction())
            {
                return context.CreatePathItemName((IEdmAction)operation);
            }

            return context.CreatePathItemName((IEdmFunction)operation);
        }

        /// <summary>
        /// Create the path item name for a <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="context">The OData context.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <param name="navigationProperty">The Edm navigation property.</param>
        /// <returns>The created path item name</returns>
        public static string CreateNavigationPathItemName(this ODataContext context,
            IEdmNavigationSource navigationSource, IEdmNavigationProperty navigationProperty)
        {
            Utils.CheckArgumentNull(context, nameof(context));
            Utils.CheckArgumentNull(navigationSource, nameof(navigationSource));
            Utils.CheckArgumentNull(navigationProperty, nameof(navigationProperty));

            string pathItemName;
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            if (entitySet != null)
            {
                pathItemName = context.CreateEntityPathName(entitySet);
            }
            else // singleton
            {
                pathItemName = "/" + navigationSource.Name;
            }

            return pathItemName + "/" + navigationProperty.Name;
        }

        private static string CreatePathItemName(this ODataContext context, IEdmFunction function)
        {
            Debug.Assert(context != null);
            Debug.Assert(function != null);

            StringBuilder functionName = new StringBuilder("/");
            if (context.Settings.UnqualifiedCall)
            {
                functionName.Append(function.Name);
            }
            else
            {
                functionName.Append(function.FullName());
            }
            functionName.Append("(");

            // Structured or collection-valued parameters are represented as a parameter alias in the path template
            // and the parameters array contains a Parameter Object for the parameter alias as a query option of type string.
            int skip = function.IsBound ? 1 : 0;
            functionName.Append(String.Join(",", function.Parameters.Skip(skip).Select(p =>
            {
                if (p.Type.IsStructured() || p.Type.IsCollection())
                {
                    return p.Name + "=@" + p.Name;
                }
                else
                {
                    return p.Name + "={" + p.Name + "}";
                }
            })));

            functionName.Append(")");

            return functionName.ToString();
        }

        private static string CreatePathItemName(this ODataContext context, IEdmAction action)
        {
            Debug.Assert(context != null);
            Debug.Assert(action != null);

            if (context.Settings.UnqualifiedCall)
            {
                return "/" + action.Name;
            }
            else
            {
                return "/" + action.FullName();
            }
        }
    }
}
