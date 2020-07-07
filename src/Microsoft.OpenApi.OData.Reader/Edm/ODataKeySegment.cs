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
        public override string Identifier
        {
            get
            {
                IList<string> keys = new List<string>();
                foreach (var key in EntityType.Key())
                {
                    keys.Add(key.Name);
                }

                return String.Join(",", keys);
            }
        }

        /// <inheritdoc />
        public override string GetPathItemName(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            Utils.CheckArgumentNull(settings, nameof(settings));

            IList<IEdmStructuralProperty> keys = EntityType.Key().ToList();
            if (keys.Count() == 1)
            {
                string keyName = keys.First().Name;

                if (settings.PrefixEntityTypeNameBeforeKey)
                {
                    string name = Utils.GetUniqueName(EntityType.Name + "-" + keyName, parameters);
                    return "{" + name + "}";
                }
                else
                {
                    string name = Utils.GetUniqueName(keyName, parameters);
                    return "{" + name + "}";
                }
            }
            else
            {
                IList<string> keyStrings = new List<string>();
                foreach (var keyProperty in keys)
                {
                    string name = Utils.GetUniqueName(keyProperty.Name, parameters);
                    keyStrings.Add(keyProperty.Name + "={" + name + "}");
                }

                return String.Join(",", keyStrings);
            }
        }

        internal IDictionary<string, string> GetKeyNameMapping(OpenApiConvertSettings settings, HashSet<string> parameters)
        {
            IDictionary<string, string> keyNamesMapping = new Dictionary<string, string>();
            IList<IEdmStructuralProperty> keys = EntityType.Key().ToList();
            if (keys.Count() == 1)
            {
                string keyName = keys.First().Name;

                if (settings.PrefixEntityTypeNameBeforeKey)
                {
                    string name = Utils.GetUniqueName(EntityType.Name + "-" + keyName, parameters);
                    keyNamesMapping[keyName] = name;
                }
                else
                {
                    string name = Utils.GetUniqueName(keyName, parameters);
                    keyNamesMapping[keyName] = name;
                }
            }
            else
            {
                foreach (var keyProperty in keys)
                {
                    string name = Utils.GetUniqueName(keyProperty.Name, parameters);
                    keyNamesMapping[keyProperty.Name] = name;
                }
            }

            return keyNamesMapping;
        }
    }
}