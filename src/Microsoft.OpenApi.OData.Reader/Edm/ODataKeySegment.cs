// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Common;

namespace Microsoft.OpenApi.OData.Edm
{
    /// <summary>
    /// The key segment.
    /// </summary>
    public class ODataKeySegment : ODataSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataKeySegment"/> class.
        /// </summary>
        /// <param name="entityType">The entity type contains the keys.</param>
        public ODataKeySegment(IEdmEntityType entityType)
        {
            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
        }

        /// <inheritdoc />
        public override IEdmEntityType EntityType { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Key;

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            IList<IEdmStructuralProperty> keys = EntityType.Key().ToList();
            if (keys.Count() == 1)
            {
                string keyName = keys.First().Name;

                if (settings.PrefixEntityTypeNameBeforeKey)
                {
                    return "{" + EntityType.Name + "-" + keyName + "}";
                }
                else
                {
                    return "{" + keyName + "}";
                }
            }
            else
            {
                IList<string> keyStrings = new List<string>();
                foreach (var keyProperty in keys)
                {
                    keyStrings.Add(keyProperty.Name + "={" + keyProperty.Name + "}");
                }

                return String.Join(",", keyStrings);
            }
        }
    }
}