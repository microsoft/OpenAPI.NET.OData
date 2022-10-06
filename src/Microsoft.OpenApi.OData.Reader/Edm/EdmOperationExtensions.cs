using System;
using Microsoft.OData.Edm;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// Extension methods for <see cref="IEdmOperation"/>
    /// </summary>
    internal static class EdmOperationExtensions
    {
        /// <summary>
        /// Checks whether the EDM is a delta function
        /// </summary>
        /// <param name="operation">The EDM operation.</param>
        /// <returns></returns>
        public static bool IsDeltaFunction(this IEdmOperation operation)
        {
            if (operation.IsFunction() && "delta".Equals(operation.Name, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}
